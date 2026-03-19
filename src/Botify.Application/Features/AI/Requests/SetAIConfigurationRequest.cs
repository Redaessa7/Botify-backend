using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Requests
{
    public sealed class SetAIConfigurationRequest
    {
        public string SystemPrompt { get; init; } = string.Empty;
        public string ModelName { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
        public AIPlatform Platform { get; init; } 
        public double Temperature { get; init; } = 0.7;
        public int MaxTokens { get; init; }

        // Feature flags
        public bool OrderProcessingEnabled { get; init; }
        public bool ProductRecommendationsEnabled { get; init; }
        public bool SmallTalkEnabled { get; init; }
        public bool AutoEscalateEnabled { get; init; }

        public Result<Success> ApplyTo(AIConfiguration config)
        {


            config.Update(
                SystemPrompt,
                ModelName,
                ApiKey,
                Platform,
                Temperature,
                MaxTokens,
                OrderProcessingEnabled,
                ProductRecommendationsEnabled,
                SmallTalkEnabled,
                AutoEscalateEnabled
            );

            return Result.Success;
        }
    }
}
