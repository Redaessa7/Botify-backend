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

        if(user.EmailConfirmed == false)
        {
            await StoreOtpAsync($"otp:{request.Email}");
            await _emailService.SendOtpAsync(request.Email, await _redis.GetStringAsync($"otp:{request.Email}"));

            return Error.Validation("Email.NotConfirmed", "Email not confirmed. OTP sent to email for verification.");
        }
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
            PhoneNumber = request.Phone,
            EmailConfirmed = false
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

        // 6. Generate and send OTP
        var otp = await StoreOtpAsync($"otp:{request.Email}");
        await _emailService.SendOtpAsync(request.Email, otp);

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
            return Error.Validation("Password.NotValid", "Invalid password.");

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
            return IdentityErrors.InvalidPassowrd;

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
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return true; // generic success

        var otp = await StoreOtpAsync($"otp:{request.Email}", ct);

        await _emailService.SendOtpAsync(request.Email, otp);

        return true;
    }

    public async Task<Result<string>> GenerateEmailChangeOtpAsync(GenerateEmailChangeOtpRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.OldEmail);
        if (user is null)
            return IdentityErrors.UserNotFound;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid || !user.Email.Equals(request.OldEmail, StringComparison.OrdinalIgnoreCase))
            return IdentityErrors.InvalidCredentials;

       var isEmailTaken = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == request.NewEmail.ToLower(), ct);

       if(isEmailTaken)
            return IdentityErrors.EmailAlreadyExists;

        var otp = await StoreOtpAsync($"otp:{request.OldEmail}", ct);

        await _emailService.SendOtpAsync(request.NewEmail, otp);

        return otp;
    }
    public async Task<Result<string>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var storedOtp = await _redis.GetStringAsync($"otp:{request.Email}");
        if (storedOtp is null || storedOtp != request.Otp)
            return Error.Validation("Otp.Invalid", "Invalid or expired OTP.");
        var user = await _userManager.FindByEmailAsync(request.Email);

        if(user is null) 
            return IdentityErrors.UserNotFound;

        // Generate reset token (GUID)
        var resetToken = Guid.NewGuid().ToString("N");
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        // Store reset token in Redis with TTL (15 minutes)
        await _redis.SetStringAsync($"reset:{request.Email}", resetToken, options,ct);
        await _redis.RemoveAsync($"otp:{request.Email}", ct);

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

    public async Task<Result<bool>> UpdateEmailAsync(UpdateEmailRequest request, CancellationToken ct = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.NewEmail) || string.IsNullOrWhiteSpace(request.OldEmail))
            return Error.Validation("Email.Required", "New email and old email are required.");

        // Validate reset token
        var storedToken = await _redis.GetStringAsync($"reset:{request.NewEmail}", ct);
        if (storedToken is null || storedToken != request.ResetToken)
            return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");

        // If old and new email are the same, just clear token
        if (request.OldEmail.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase))
        {
            await _redis.RemoveAsync($"reset:{request.NewEmail}", ct);
            return true;
        }

        // Ensure new email is not already taken
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.NewEmail, ct))
            return IdentityErrors.EmailAlreadyExists;

        // Find user by old email
        var user = await _userManager.FindByEmailAsync(request.OldEmail);
        if (user is null)
            return IdentityErrors.UserNotFound;

        // Validate current password
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid || !user.Email.Equals(request.OldEmail, StringComparison.OrdinalIgnoreCase))
            return IdentityErrors.InvalidCredentials;

        // Generate and apply email change token
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        var result = await _userManager.ChangeEmailAsync(user, request.NewEmail, token);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return errors;
        }
        user.EmailConfirmed = true;// Since OTP verified, we can consider email as confirmed 
        var usernameResult = await _userManager.SetUserNameAsync(user, request.NewEmail);

        if (!usernameResult.Succeeded)
        {
            var errors = usernameResult.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return errors;
        }
        // Clear reset token after successful change
        await _redis.RemoveAsync($"reset:{request.NewEmail}", ct);


        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.Email == request.OldEmail, ct);
        if (profile != null)
        {
           var profileResult =  profile.UpdateEmail(request.NewEmail);

           if(!profileResult.IsSuccess)
                return profileResult.Errors;

            await _dbContext.SaveChangesAsync(ct);
        }

        return true;
    }
    public async Task<Result<Success>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        // Check reset token in Redis
        var storedToken = await _redis.GetStringAsync($"reset:{request.Email}", ct);
        if (storedToken is null || storedToken != request.ResetToken)
            return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return IdentityErrors.UserNotFound;

        // Confirm email
        user.EmailConfirmed = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors
                .Select(e => Error.Validation(e.Code, e.Description))
                .ToList();
            return errors;
        }

        // Remove reset token after success
        await _redis.RemoveAsync($"reset:{request.Email}", ct);

        return Result.Success;
    }

    private string GenerateOtp()
    {
        return new Random().Next(100000, 999999).ToString();
    }
    private async Task<string> StoreOtpAsync(string key, CancellationToken ct = default)
    {
        // Generate a 6-digit OTP
        var otp = GenerateOtp();

        // Store OTP in Redis with TTL (10 minutes)
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        await _redis.SetStringAsync(key, otp, options, ct);

        return otp;
    }

}
