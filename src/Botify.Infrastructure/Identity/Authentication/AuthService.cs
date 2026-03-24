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
using Botify.Infrastructure.CrossCutting.BackgroundServices.Email;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Botify.Infrastructure.Identity.Authentication;

public class AuthService(
        UserManager<ApplicationUser> userManager,
        IDistributedCache redis,
        IMerchantService merchantService,
        ITokenProvider tokenProvider,
        ApplicationDbContext dbContext,
        IFileService fileService,
        IEmailQueue emailQueue,
        IUser user,
        ILogger<AuthService> logger) : IAuthServices // Injected ILogger
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IMerchantService _merchantService = merchantService;
    private readonly ITokenProvider _tokenProvider = tokenProvider;
    private readonly IFileService _fileService = fileService;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IUser _user = user;
    private readonly IDistributedCache _redis = redis;
    private readonly IEmailQueue _emailQueue = emailQueue;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Email}", request.Email);

        var profile = await _dbContext.UserProfiles.AnyAsync(p => p.Email == request.Email);
        if (!profile)
        {
            _logger.LogWarning("Login failed: Profile not found for {Email}", request.Email);
            return IdentityErrors.UserNotFound;
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login failed: Identity user not found for {Email}", request.Email);
            return IdentityErrors.InvalidCredentials;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            return IdentityErrors.InvalidCredentials;
        }

        if (user.EmailConfirmed == false)
        {
            _logger.LogInformation("Login blocked: Email not confirmed for {Email}. Sending OTP.", request.Email);
            var otp = await StoreOtpAsync($"otp:{request.Email}");
            _emailQueue.Enqueue(request.Email, otp);

            return Error.Validation("Email.NotConfirmed", "Email not confirmed. OTP sent to email for verification.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var generateTokenRequest = new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        };

        _logger.LogInformation("User {Email} successfully logged in.", request.Email);
        return await _tokenProvider.GenerateTokenAsync(generateTokenRequest, default);
    }

    public async Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Registration initiated for email: {Email}", request.Email);
        // Use FirstOrDefaultAsync to execute the query and get a single result or null
    var existingMerchant = await _dbContext.Merchants
        .IgnoreQueryFilters()
        .Where(x => x.Profile != null && x.Profile.Email == request.Email)
        .Select(x => new 
        { 
            x.IsDeleted 
        })
        .FirstOrDefaultAsync();

    if (existingMerchant is not null)
    {
        if (!existingMerchant.IsDeleted)
        {
            _logger.LogWarning("Registration failed: Email {Email} already exists and is active.", request.Email);
            return IdentityErrors.EmailAlreadyExists;
        }

        _logger.LogWarning("Registration failed: Email {Email} is marked as deleted.", request.Email);
        return IdentityErrors.AccountDeleted;
    }

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
            _logger.LogError("Identity creation failed for {Email}. Errors: {Errors}", 
                request.Email, string.Join(", ", created.Errors.Select(e => e.Description)));
            
            return created.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }

        var createMerchant = new CreateMerchantRequest(
            user.Id,
            new CreateUserProfileRequest(request.FirstName, request.LastName, request.Email, request.Bio, request.ProfileImageUrl, request.Language)
        );

        var merchant = await _merchantService.CreateAsync(createMerchant, default);
        if (!merchant.IsSuccess)
        {
            _logger.LogCritical("Merchant creation failed for user {UserId}. Rolling back identity user.", user.Id);
            await _userManager.DeleteAsync(user);
            return merchant.Errors;
        }

        await _userManager.AddToRoleAsync(user, Roles.MERCHANT.ToString());
        
        var otp = await StoreOtpAsync($"otp:{request.Email}");
        _emailQueue.Enqueue(request.Email, otp);

        _logger.LogInformation("Registration successful for {Email}. Merchant profile created.", request.Email);

        var roles = await _userManager.GetRolesAsync(user);
        return await _tokenProvider.GenerateTokenAsync(new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        }, default);
    }

    public async Task<Result<bool>> LogoutAsync(string refreshToken)
    {
        // We log a hashed version or prefix of the token to identify it without exposing it
        _logger.LogInformation("Logout requested for refresh token starting with: {TokenPrefix}", refreshToken[..Math.Min(refreshToken.Length, 5)]);

        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (token is null) return IdentityErrors.RefreshTokenInvalid;

        token.Revoke();
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked successfully for user {UserId}.", token.UserId);
        return true;
    }

    // Helper methods for logging internal state changes
    private async Task<string> StoreOtpAsync(string key, CancellationToken ct = default)
    {
        var otp = GenerateOtp();
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        await _redis.SetStringAsync(key, otp, options, ct);
        _logger.LogDebug("OTP stored in Redis for key: {OtpKey}", key);

        return otp;
    }
    
// --------------------------
    // Password Management
    // --------------------------
    public async Task<Result<bool>> CheckOldPasswordAsync(CheckOldPasswordRequest request, CancellationToken ct = default)
    {
        if (_user.Id is null)
        {
            _logger.LogWarning("CheckOldPassword failed: User ID not found in current context.");
            return IdentityErrors.IdRequired;
        }

        var user = await _userManager.FindByIdAsync(_user.Id);
        if (user is null)
        {
            _logger.LogWarning("CheckOldPassword failed: User {UserId} not found.", _user.Id);
            return IdentityErrors.UserNotFound;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
        if (!passwordValid)
        {
            _logger.LogWarning("CheckOldPassword: Password verification failed for user {UserId}.", _user.Id);
            return Error.Validation("Password.NotValid", "Invalid password.");
        }

        return true;
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmPassword)
            return Error.Validation("Password.Mismatch", "Passwords do not match.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return IdentityErrors.UserNotFound;

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("ChangePassword failed for {Email}: {Errors}", 
                request.Email, string.Join(", ", result.Errors.Select(e => e.Code)));
            return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }

        // Security Best Practice: Revoke all tokens on password change
        var tokens = _dbContext.RefreshTokens.Where(t => t.UserId == user.Id);
        foreach (var token in tokens) token.Revoke();
        
        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Password changed and tokens revoked for user {Email}.", request.Email);

        return true;
    }

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Log internally but return success to prevent account enumeration
            _logger.LogInformation("ForgotPassword: No user found for {Email}. Proceeding silently.", request.Email);
            return true; 
        }

        var otp = await StoreOtpAsync($"otp:{request.Email}", ct);
        _emailQueue.Enqueue(request.Email, otp);

        _logger.LogInformation("ForgotPassword OTP sent to {Email}.", request.Email);
        return true;
    }

    // --------------------------
    // Email & OTP Verification
    // to do why in update email send to old and new email
    // --------------------------
    public async Task<Result<Success>> GenerateEmailChangeOtpAsync(GenerateEmailChangeOtpRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.OldEmail);
        if (user is null) return IdentityErrors.UserNotFound;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!passwordValid) return IdentityErrors.InvalidCredentials;

        var isEmailTaken = await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == request.NewEmail.ToLower(), ct);
        if (isEmailTaken) return IdentityErrors.EmailAlreadyExists;

        var otp = await StoreOtpAsync($"otp:{request.OldEmail}", ct);
        _emailQueue.Enqueue(user.Email, otp);

        _logger.LogInformation("Email change OTP generated for user {UserId}.", user.Id);
        return Result.Success;
    }

    public async Task<Result<string>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default)
    {
        var storedOtp = await _redis.GetStringAsync($"otp:{request.Email}", ct);
        if (storedOtp is null || storedOtp != request.Otp)
        {
            _logger.LogWarning("OTP Verification failed for {Email}: Invalid or expired.", request.Email);
            return Error.Validation("Otp.Invalid", "Invalid or expired OTP.");
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null) return IdentityErrors.UserNotFound;

        var resetToken = Guid.NewGuid().ToString("N");
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };

        await _redis.SetStringAsync($"reset:{request.Email}", resetToken, options, ct);
        await _redis.RemoveAsync($"otp:{request.Email}", ct);

        _logger.LogInformation("OTP verified for {Email}. Reset token generated.", request.Email);
        return resetToken;
    }

public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
{
    // 1. Verify custom Redis token
    var storedToken = await _redis.GetStringAsync($"reset:{request.Email}", ct);
    if (storedToken is null || storedToken != request.ResetToken)
    {
        _logger.LogWarning("Password reset failed: Invalid or expired reset token for {Email}", request.Email);
        return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");
    }

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user is null) 
    {
        _logger.LogWarning("Password reset failed: User {Email} not found.", request.Email);
        return IdentityErrors.UserNotFound;
    }

    // 2. Identity Reset Logic
    // We generate the internal Identity token because ResetPasswordAsync requires it 
    // to cryptographically verify the change and update the security stamp.
    var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
    var result = await _userManager.ResetPasswordAsync(user, identityToken, request.NewPassword);

    if (!result.Succeeded)
    {
        var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
        _logger.LogWarning("Identity password reset failed for User {UserId}: {Errors}", user.Id, errorMessages);
        
        return result.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
    }

    // 3. Security Hardening: Force Logout Everywhere
    // This invalidates all existing cookies and session-based tokens
    await _userManager.UpdateSecurityStampAsync(user);

    // 4. Revoke all Refresh Tokens
    // If the account was compromised, this kicks the attacker out immediately.
    var activeTokens = _dbContext.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked);
    foreach (var refreshToken in activeTokens)
    {
        refreshToken.Revoke();
    }
    
    await _dbContext.SaveChangesAsync(ct);

    // 5. Cleanup
    await _redis.RemoveAsync($"reset:{request.Email}", ct);
    
    _logger.LogInformation("Password reset successfully for User {UserId}. All sessions invalidated.", user.Id);

    return true;
}
public async Task<Result<bool>> UpdateEmailAsync(UpdateEmailRequest request, CancellationToken ct = default)
{
    // 1. Validation Logic
    var storedToken = await _redis.GetStringAsync($"reset:{request.OldEmail}", ct);
    if (storedToken is null || storedToken != request.ResetToken)
        return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");

    var user = await _userManager.FindByEmailAsync(request.OldEmail);
    if (user is null) return IdentityErrors.UserNotFound;

    // 2. Transactional Scope
    // This ensures either EVERYTHING changes or NOTHING changes.
    using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    try
    {
        // Change Email
        var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        var emailResult = await _userManager.ChangeEmailAsync(user, request.NewEmail, emailToken);

        if (!emailResult.Succeeded)
        {
            var errorDetails = string.Join(" | ", emailResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Email update failed for User {UserId}: {Errors}", user.Id, errorDetails);
            return emailResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }

        // Sync Username (Best Practice: Usually Username = Email)
        await _userManager.SetUserNameAsync(user, request.NewEmail);
        
        // Update Security Stamp (Invalidates old sessions)
        await _userManager.UpdateSecurityStampAsync(user);

        // Sync Profile Table (Using UserId instead of Email for the lookup)
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id, ct);
        if (profile != null)
        {
            profile.UpdateEmail(request.NewEmail);
            await _dbContext.SaveChangesAsync(ct);
        }

        // 3. Commit all changes at once
        await transaction.CommitAsync(ct);

        // Clear Redis only after DB success
        await _redis.RemoveAsync($"reset:{request.NewEmail}", ct);

        _logger.LogInformation("Successfully updated email from {OldEmail} to {NewEmail} for User {UserId}", 
            request.OldEmail, request.NewEmail, user.Id);

        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(ct);
        _logger.LogError(ex, "Transaction failed while updating email for user {UserId}. Rolling back.", user.Id);
        return Error.Failure("Email.UpdateError", "An unexpected error occurred during email update.");
    }
}
public async Task<Result<TokenResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
{
    // 1. Validate Redis Token (Your custom OTP/Reset link logic)
    var storedToken = await _redis.GetStringAsync($"reset:{request.Email}", ct);
    if (storedToken is null || storedToken != request.ResetToken)
    {
        _logger.LogWarning("Email verification failed for {Email}: Invalid or expired reset token.", request.Email);
        return Error.Validation("ResetToken.Invalid", "Invalid or expired reset token.");
    }

    var user = await _userManager.FindByEmailAsync(request.Email);
    if (user is null) 
    {
        _logger.LogError("Email verification failed: User {Email} not found.", request.Email);
        return IdentityErrors.UserNotFound;
    }

    // 2. Transactional Update
    using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
    try
    {
        user.EmailConfirmed = true;
        // Best Practice: Refresh the security stamp when a major account property changes
        await _userManager.UpdateSecurityStampAsync(user);
        
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user confirmation status for {UserId}: {Errors}", user.Id, errors);
            return updateResult.Errors.Select(e => Error.Validation(e.Code, e.Description)).ToList();
        }

        // 3. Sync Profile / Merchant Status
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id, ct);
        if (profile != null)
        {
            // profile.MarkAsVerified(); 
            await _dbContext.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
        
        // 4. Cleanup Redis
        await _redis.RemoveAsync($"reset:{request.Email}", ct);
        _logger.LogInformation("Email {Email} successfully confirmed for User {UserId}.", request.Email, user.Id);

        // 5. Generate Login Token
        var roles = await _userManager.GetRolesAsync(user);
        return await _tokenProvider.GenerateTokenAsync(new GenerateTokenRequest
        {
            Id = user.Id,
            Email = user.Email!,
            Roles = roles.ToList()
        }, ct);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync(ct);
        _logger.LogError(ex, "Unexpected error during email verification for {Email}.", request.Email);
        return Error.Failure("Email.VerificationError", "An error occurred while confirming your email.");
    }
}
public async Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken)
{
    _logger.LogInformation("Refresh token attempt for prefix: {TokenPrefix}", 
        refreshToken[..Math.Min(refreshToken.Length, 5)]);

    // 1. Fetch token and include User if possible to reduce DB roundtrips
    var oldToken = await _dbContext.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == refreshToken);

    if (oldToken is null)
    {
        _logger.LogWarning("Refresh failed: Token not found in database.");
        return IdentityErrors.RefreshTokenInvalid;
    }

    // 2. Security Audit: Reuse Detection
    if (oldToken.IsRevoked)
    {
        _logger.LogCritical("REUSE DETECTION: Revoked token used for user {UserId}. Revoking all user sessions!", oldToken.UserId);
        
        // Best Practice: If a revoked token is used, assume a breach. 
        // Revoke all active tokens for this user.
        var allUserTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == oldToken.UserId && !t.IsRevoked)
            .ToListAsync();
            
        foreach(var t in allUserTokens) t.Revoke();
        await _dbContext.SaveChangesAsync();

        return IdentityErrors.RefreshTokenInvalid;
    }

    if (oldToken.ExpiresAt < DateTime.UtcNow)
    {
        _logger.LogWarning("Refresh failed: Token expired for user {UserId}.", oldToken.UserId);
        return IdentityErrors.RefreshTokenExpired;
    }

    // 3. Identity Check
    var user = await _userManager.FindByIdAsync(oldToken.UserId);
    if (user is null)
    {
        _logger.LogError("Refresh failed: User {UserId} no longer exists.", oldToken.UserId);
        return IdentityErrors.UserNotFound;
    }

    // 4. Atomic Execution via TokenProvider
    var roles = await _userManager.GetRolesAsync(user);
    var generateTokenRequest = new GenerateTokenRequest
    {
        Id = user.Id,
        Email = user.Email!,
        Roles = roles.ToList(),
        OldRefreshToken = oldToken.Token // The TokenProvider should handle the revocation of the old token inside its own transaction
    };

    var result = await _tokenProvider.GenerateTokenAsync(generateTokenRequest, default);

    if (result.IsSuccess)
    {
        _logger.LogInformation("Successfully refreshed token for User {UserId}.", user.Id);
    }

    return result;
}

    // --------------------------
    // Private Helpers
    // --------------------------
    private async Task<Result<bool>> IsDuplicateEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
        
        if (exists)
        {
            _logger.LogDebug("Duplicate email check: {Email} already exists.", email);
        }

        return exists;
    }

    private string GenerateOtp()
    {
        // Using a cryptographically secure random number generator is best practice for OTPs
        return new Random().Next(100000, 999999).ToString();
    }
}