using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Requests
{
    public sealed class UpdateAIConfigurationRequest
    {
        public string SystemPrompt { get; init; } = string.Empty;
        public string ModelName { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
        public string Platform { get; init; }
        public double Temperature { get; init; } = 0.7;
        public int MaxTokens { get; init; }

        public Result<Success> ApplyTo(AIConfiguration config)
        {
            var platformResult = Enum.TryParse<AIPlatform>(Platform,true, out var parsedPlatform);
            // You can add validation here if needed
            config.Update(SystemPrompt, ModelName, ApiKey, parsedPlatform, Temperature, MaxTokens);
            return Result.Success;
        }
    }
}
