using Botify.Application.Requests;
using Botify.Application.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Services.Auths;

public interface IAuthServices
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);

    Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    Task<Result<bool>> LogoutAsync(LogoutRequest request);
}
