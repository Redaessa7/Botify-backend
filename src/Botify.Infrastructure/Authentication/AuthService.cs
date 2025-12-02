using Botify.Application.Common.Interfaces;
using Botify.Application.Requests;
using Botify.Application.Responses;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Authentication;

public class AuthService(
        UserManager<ApplicationUser> userManager,
        IMerchantService merchantService,
        ITokenProvider tokenProvider,
        ApplicationDbContext dbContext,
        IFileService fileService) : IAuthServices
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMerchantService _merchantService = merchantService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IFileService _fileService = fileService;
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
            Roles = roles.ToList()
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest,default);
    }



    // --------------------------
    // Refresh Token
    // --------------------------
    public async Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken)
    {
        var oldToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t=> t.Token == refreshToken);
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
            Roles = roles.ToList(),
            OldRefreshToken = oldToken.Token
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest,default);
    }
    public async Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request)
    {
        // 1. Create Identity user
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            PhoneNumber = request.Phone
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            var errors = created.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        // 2. Handle file uploads (Logo + ProfileImage)
        string? logoUrl = null;
        if (request.Logo is not null)
        {
            if (!_fileService.ValidateFile(request.Logo.FileName, request.Logo.ContentType, request.Logo.Length))
            {
                await _userManager.DeleteAsync(user);
                return Error.Validation("File.InvalidLogo", "Logo file is invalid.");
            }

            var logoResult = await _fileService.SaveFileAsync(request.Logo.OpenReadStream(), request.Logo.FileName, "logos");
            if (!logoResult.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
                return logoResult.Errors;
            }

            logoUrl = logoResult.Value;
        }

        string? profileImageUrl = null;
        if (request.ProfileImage is not null)
        {
            if (!_fileService.ValidateFile(request.ProfileImage.FileName, request.ProfileImage.ContentType, request.ProfileImage.Length))
            {
                await _userManager.DeleteAsync(user);
                return Error.Validation("File.InvalidProfileImage", "Profile image file is invalid.");
            }

            var profileResult = await _fileService.SaveFileAsync(request.ProfileImage.OpenReadStream(), request.ProfileImage.FileName, "profiles");
            if (!profileResult.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
                return profileResult.Errors;
            }

            profileImageUrl = profileResult.Value;
        }

        // 3. Create merchant + profile
        var createMerchant = new CreateMerchantRequest(
            user.Id,
            request.MerchantName,
            request.Industry,
            logoUrl,
            new CreateUserProfileRequest(
                request.FirstName,
                request.LastName,
                request.Bio,
                profileImageUrl,
                request.Language
            )
        );

        var merchant = await _merchantService.CreateAsync(createMerchant, default);
        if (!merchant.IsSuccess)
        {
            await _userManager.DeleteAsync(user);
            return merchant.Errors;
        }

        // 4. Assign role
        await _userManager.AddToRoleAsync(user, Roles.Merchant.ToString());

        var roles = await _userManager.GetRolesAsync(user);

        // 5. Generate token
        var generateTokenRequest = new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest, default);
    }
    // --------------------------
    // Logout = revoke refresh token
    // --------------------------
    public async Task<Result<bool>> LogoutAsync(string refreshToken)
    {
        var token =  await _dbContext.RefreshTokens.FirstOrDefaultAsync(t=> t.Token == refreshToken);

        if (token is null)
            return IdentityErrors.RefreshTokenInvalid;

        token.Revoke();
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
