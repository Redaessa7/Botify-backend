using System.Threading.Tasks;
using Botify.Application.Features.AI.Requests;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Strategies
{
    public interface IAIPlatformStrategy
    {
        /// <summary>
        /// Validates the API token and optionally checks if a model exists on the platform.
        /// Returns Result.Success if valid, or an Error from AIConfigurationErrors if invalid.
        /// </summary>
        Task<Result<Success>> ValidateTokenAsync(TokenValidationRequest request);
    }
}
