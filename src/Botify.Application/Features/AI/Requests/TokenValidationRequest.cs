using Botify.Domain.Models.AI.Enums;

namespace Botify.Application.Features.AI.Requests
{
    public sealed class TokenValidationRequest
    {
        public string ApiKey { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public AIPlatform Platform { get; init; }
        public string? BaseUrl { get; init; } // required for Azure
    }
}
