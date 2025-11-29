using Botify.Application.Common.Interfaces;
using Botify.Application.Requests;
using Botify.Application.Responses;
using Botify.Application.Services.Auths;
using Botify.Domain.Identity;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Authentication;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenProvider tokenProvider,
    ApplicationDbContext dbContext) : IAuthServices
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly ApplicationDbContext _dbContext = dbContext;

    // --------------------------
    // Login
    // --------------------------
    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return IdentityErrors.InvalidCredentials;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return IdentityErrors.InvalidCredentials;

        var roles = await _userManager.GetRolesAsync(user);

        var generateTokenRequest = new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName!,
            Roles = roles.ToList()
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest,default);
    }

    // --------------------------
    // Register
    // --------------------------
    public async Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Error.Unexpected();

        // default role (e.g. Merchant)
        await _userManager.AddToRoleAsync(user, "User");

        var roles = await _userManager.GetRolesAsync(user);

        var generateTokenRequest = new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            Roles = roles.ToList()
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest,default);
    }

    // --------------------------
    // Refresh Token
    // --------------------------
    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var oldToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t=> t.Token == request.RefreshToken);
        if (oldToken is null)
            return IdentityErrors.RefreshTokenInvalid;

        if (oldToken.IsRevoked || oldToken.ExpiresAt < DateTime.UtcNow)
            return IdentityErrors.RefreshTokenExpired;

        var user = await _userManager.FindByIdAsync(oldToken.UserId);
        if (user is null)
            return IdentityErrors.UserNotFound;

        var roles = await _userManager.GetRolesAsync(user);

        var generateTokenRequest = new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName!,
            LastName = user.LastName!,
            Roles = roles.ToList(),
            OldRefreshToken = oldToken.Token
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest,default);
    }

    // --------------------------
    // Logout = revoke refresh token
    // --------------------------
    public async Task<Result<bool>> LogoutAsync(LogoutRequest request)
    {
        var token =  await _dbContext.RefreshTokens.FirstOrDefaultAsync(t=> t.Token == request.RefreshToken);

        if (token is null)
            return IdentityErrors.RefreshTokenInvalid;

        token.Revoke();
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
