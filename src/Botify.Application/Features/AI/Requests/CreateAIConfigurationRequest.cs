using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;

namespace Botify.Application.Features.AI.Requests
{
    public sealed class CreateAIConfigurationRequest
    {
        public string SystemPrompt { get; init; } = string.Empty;
        public string ModelName { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
        public string Platform { get; init; }
        public double Temperature { get; init; } = 0.7;
        public int MaxTokens { get; init; }

        public AIConfiguration ToDomain(Guid merchantId)
        {
            var platform = Enum.Parse<AIPlatform>(Platform, true);
            return AIConfiguration.Create(
                Guid.NewGuid(),
                SystemPrompt,
                ModelName,
                ApiKey,
                platform,
                Temperature,
                MaxTokens,
                merchantId
            ).Value;
        }
    }
}
