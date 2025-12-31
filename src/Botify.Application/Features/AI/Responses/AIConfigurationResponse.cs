using Botify.Domain.Models.AI;

namespace Botify.Application.Features.AI.Responses
{
    public sealed class AIConfigurationResponse
    {
        public Guid Id { get; init; }
        public Guid MerchantId { get; init; }
        public string SystemPrompt { get; init; } = string.Empty;
        public string ModelName { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public double Temperature { get; init; }
        public int MaxTokens { get; init; }
        public DateTime CreateAt { get; init; }
        public DateTime? UpdateAt { get; init; }

        public static AIConfigurationResponse FromDomain(AIConfiguration config)
        {
            return new AIConfigurationResponse
            {
                Id = config.Id,
                MerchantId = config.MerchantId,
                SystemPrompt = config.SystemPrompt,
                ModelName = config.ModelName,
                ApiKey = config.ApiKey,
                Platform = config.Platform.ToString(),
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens,
                CreateAt = config.CreatedAtUtc,
                UpdateAt = config.LastModifiedUtc
            };
        }
    }
}
