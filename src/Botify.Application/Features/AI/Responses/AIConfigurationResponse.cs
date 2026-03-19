using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using System;

namespace Botify.Application.Features.AI.Responses
{
    public sealed class AIConfigurationResponse
    {
        public Guid Id { get; init; }
        public Guid MerchantId { get; init; }
        public string SystemPrompt { get; init; } = string.Empty;
        public string ModelName { get; init; } = string.Empty;
        public string ApiKey { get; init; } = string.Empty;
        public AIPlatform Platform { get; init; } 
        public double Temperature { get; init; }
        public int MaxTokens { get; init; }

        // Feature flags
        public bool OrderProcessingEnabled { get; init; }
        public bool ProductRecommendationsEnabled { get; init; }
        public bool SmallTalkEnabled { get; init; }
        public bool AutoEscalateEnabled { get; init; }

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
                Platform = config.Platform,
                Temperature = config.Temperature,
                MaxTokens = config.MaxTokens,
                OrderProcessingEnabled = config.OrderProcessingEnabled,
                ProductRecommendationsEnabled = config.ProductRecommendationsEnabled,
                SmallTalkEnabled = config.SmallTalkEnabled,
                AutoEscalateEnabled = config.AutoEscalateEnabled,
                CreateAt = config.CreatedAtUtc,
                UpdateAt = config.LastModifiedUtc
            };
        }
    }
}
