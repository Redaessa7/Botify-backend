using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Requests;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Auths.Requests;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Services.Auths;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Botify.Infrastructure.Identity.Authentication;

public class AuthService(
        UserManager<ApplicationUser> userManager,
        IDistributedCache redis,
        IMerchantService merchantService,
        ITokenProvider tokenProvider,
        ApplicationDbContext dbContext,
        IFileService fileService,
        IEmailSender emailService,
        IUser user) : IAuthServices
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMerchantService _merchantService = merchantService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IFileService _fileService = fileService;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IUser _user = user;
    private readonly IDistributedCache _redis = redis;
    private readonly IEmailSender _emailService = emailService;
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

    private async Task<Result<bool>> IsDuplicateEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        return exists;
    }
    public async Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request)
    {
        if ((await IsDuplicateEmailAsync(request.Email)).Value)
        {
            return IdentityErrors.EmailAlreadyExists;
        }
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


        // 3. Create merchant + profile
        var createMerchant = new CreateMerchantRequest(
            user.Id,
            new CreateUserProfileRequest(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Bio,
                request.ProfileImageUrl,
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
        await _userManager.AddToRoleAsync(user, Roles.MERCHANT.ToString());

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

    public async Task<Result<bool>> CheckOldPasswordAsync(CheckOldPasswordRequest request, CancellationToken ct = default)
    {
        if(_user.Id is null)
            return IdentityErrors.IdRequired; 
        var user = await _userManager.FindByIdAsync(_user.Id);

        // Get current user from context
         if (user is null)
            return IdentityErrors.UserNotFound;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!passwordValid)
            return IdentityErrors.InvalidCredentials;

        return true;
    }
    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Error.Validation("Password.Mismatch", "New password and confirm password do not match.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return IdentityErrors.UserNotFound;

        var oldPasswordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!oldPasswordValid)
            return IdentityErrors.InvalidCredentials;

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();

            return errors;
        }

        // Invalidate refresh tokens for security
        var tokens = _dbContext.RefreshTokens.Where(t => t.UserId == user.Id);
        foreach (var token in tokens)
            token.Revoke();
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }
    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        // Always return generic response to prevent user enumeration
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return true; // generic success

        // Generate OTP
        var otp = new Random().Next(100000, 999999).ToString();
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
        // Store OTP in Redis with TTL (5-10 minutes)
        await _redis.SetStringAsync($"otp:{request.Email}", otp, options, ct);

        // Send OTP via email/SMS
        await _emailService.SendOtpAsync(request.Email, otp);

        return true;
    }
    public async Task<Result<string>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var storedOtp = await _redis.GetStringAsync($"otp:{request.Email}");
        if (storedOtp is null || storedOtp != request.Otp)
            return Error.Validation("Otp.Invalid", "Invalid or expired OTP.");

        // Generate reset token (GUID)
        var resetToken = Guid.NewGuid().ToString("N");
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        // Store reset token in Redis with TTL (15 minutes)
        await _redis.SetStringAsync($"reset:{request.Email}", resetToken, options,ct);

        return resetToken;
    }
    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Error.Validation("Password.Mismatch", "New password and confirm password do not match.");

        var storedToken = await _redis.GetStringAsync($"reset:{request.Email}");
        if (storedToken is null || storedToken != request.ResetToken)
            return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return IdentityErrors.UserNotFound;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return errors;
        }

        // Delete reset token immediately
        await _redis.RemoveAsync($"reset:{request.Email}");

        return true;
    }

}
