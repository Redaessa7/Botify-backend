using Botify.Application.CrossCutting.Requests;
using Botify.Application.CrossCutting.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Services.Auths;

public interface IAuthServices
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);

    Task<Result<TokenResponse>> RegisterAsync(RegisterRequest request);

    Task<Result<TokenResponse>> RefreshTokenAsync(string refreshToken);

    Task<Result<bool>> LogoutAsync(string refreshToken);
}
