using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Botify.Application.Common.Interfaces;
using Botify.Application.Requests;
using Botify.Application.Responses;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Botify.Infrastructure.Authentication;

public class JwtTokenProvider(
    IConfiguration configuration,
    ILogger<JwtTokenProvider> logger,
    ApplicationDbContext dbContext
) : ITokenProvider
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<JwtTokenProvider> _logger = logger;
    private readonly ApplicationDbContext _DbContext = dbContext;

    public async Task<Result<TokenResponse>> GenerateTokenAsync(GenerateTokenRequest request,CancellationToken cancellationToken)
    {
        var jwtOptions = _configuration.GetSection("JWTOptions");
        var issuer = jwtOptions["Issuer"];
        var audience = jwtOptions["Audience"];
        var key = jwtOptions["SecurityKey"];
        var accessExpireMinutes = int.Parse(jwtOptions["TokenExpirationInMinutes"]!);
        var refreshExpireDays = int.Parse(jwtOptions["RefreshTokenExpirationInDays"]!);

        var accessExpiresAt = DateTime.UtcNow.AddMinutes(accessExpireMinutes);

        // ---------------------- JWT Claims ----------------------
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Id!),
            new(JwtRegisteredClaimNames.Email, request.Email!),
        };

        foreach (var role in request.Roles)
            claims.Add(new(ClaimTypes.Role, role));

        // ---------------------- JWT Token ----------------------
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = issuer,
            Audience = audience,
            Expires = accessExpiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.CreateToken(descriptor);
        var accessToken = tokenHandler.WriteToken(jwtToken);

        // ---------------------- Generate Refresh Token ----------------------
        string refreshToken = GenerateSecureRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshExpireDays);

        // revoke & delete old refresh token
        if (!string.IsNullOrEmpty(request.OldRefreshToken))
        {
            var old = await _DbContext.RefreshTokens.FirstOrDefaultAsync(old => old.Token == request.OldRefreshToken,cancellationToken);
            if (old != null) old.Revoke();
        }

        // save new refresh token
        var refreshTokenEntity = new RefreshToken(
            userId: request.Id!,
            token: refreshToken,
            expiresAt: refreshExpiresAt
        );

        await _DbContext.RefreshTokens.AddAsync(refreshTokenEntity,cancellationToken);
        await _DbContext.SaveChangesAsync(cancellationToken);

        // ---------------------- Response ----------------------
        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expires = accessExpiresAt
        };
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
