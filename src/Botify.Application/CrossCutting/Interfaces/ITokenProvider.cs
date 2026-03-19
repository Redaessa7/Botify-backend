using Botify.Application.CrossCutting.Requests;
using Botify.Application.CrossCutting.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.CrossCutting.Interfaces;

public interface ITokenProvider
{
    Task<Result<TokenResponse>> GenerateTokenAsync(GenerateTokenRequest request,CancellationToken cancellationToken);
}