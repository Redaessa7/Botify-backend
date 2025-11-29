using Botify.Application.Requests;
using Botify.Application.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Common.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateTokenAsync(GenerateTokenRequest request,CancellationToken cancellationToken);
}