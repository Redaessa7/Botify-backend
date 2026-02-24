using Botify.Application.CrossCutting.Requests;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Auths.Requests;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Services.Auths;

public interface IAuthServices
{
    Task<Result<TokenResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default);
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);
     Task<Result<bool>> CheckOldPasswordAsync(CheckOldPasswordRequest request, CancellationToken ct = default);
    Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default);
    Task<Result<string>> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken ct = default);
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
     Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<Result<bool>> UpdateEmailAsync(UpdateEmailRequest request, CancellationToken ct = default);
    Task<Result<string>> GenerateEmailChangeOtpAsync(GenerateEmailChangeOtpRequest request, CancellationToken ct = default);

    Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken);

    Task<Result<bool>> LogoutAsync(string refreshToken);
}
