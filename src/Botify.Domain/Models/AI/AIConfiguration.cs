using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;
using System;

namespace Botify.Domain.Models.AI
{
    public sealed class AIConfiguration : AuditableEntity
    {
        public string SystemPrompt { get; private set; } = string.Empty;
        public string ModelName { get; private set; } = string.Empty;
        public string ApiKey { get; private set; } = string.Empty;
        public AIPlatform Platform { get; private set; }
        public double Temperature { get; private set; } = 0.7;
        public int MaxTokens { get; private set; }

        // Feature flags
        public bool OrderProcessingEnabled { get; private set; } = true;
        public bool ProductRecommendationsEnabled { get; private set; } = true;
        public bool SmallTalkEnabled { get; private set; } = true;
        public bool AutoEscalateEnabled { get; private set; } = true;

        public Guid MerchantId { get; private set; }

        private AIConfiguration() { }

        private AIConfiguration(
            Guid id,
            string systemPrompt,
            string modelName,
            string apiKey,
            AIPlatform platform,
            double temperature,
            int maxTokens,
            Guid merchantId,
            bool orderProcessingEnabled,
            bool productRecommendationsEnabled,
            bool smallTalkEnabled,
            bool autoEscalateEnabled) : base(id)
        {
            SystemPrompt = systemPrompt;
            ModelName = modelName;
            ApiKey = apiKey;
            Platform = platform;
            Temperature = temperature;
            MaxTokens = maxTokens;
            MerchantId = merchantId;

            OrderProcessingEnabled = orderProcessingEnabled;
            ProductRecommendationsEnabled = productRecommendationsEnabled;
            SmallTalkEnabled = smallTalkEnabled;
            AutoEscalateEnabled = autoEscalateEnabled;
        }

        public static Result<AIConfiguration> Create(
            Guid id,
            string systemPrompt,
            string modelName,
            string apiKey,
            AIPlatform platform,
            double temperature,
            int maxTokens,
            Guid merchantId,
            bool orderProcessingEnabled = false,
            bool productRecommendationsEnabled = false,
            bool smallTalkEnabled = false,
            bool autoEscalateEnabled = false)
        {
            var validation = ValidateInputs(id, systemPrompt, modelName, apiKey, platform, temperature, maxTokens, merchantId);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new AIConfiguration(
                id,
                systemPrompt,
                modelName,
                apiKey,
                platform,
                temperature,
                maxTokens,
                merchantId,
                orderProcessingEnabled,
                productRecommendationsEnabled,
                smallTalkEnabled,
                autoEscalateEnabled);
        }

        public Result<Updated> Update(
            string systemPrompt,
            string modelName,
            string apiKey,
            AIPlatform platform,
            double temperature,
            int maxTokens,
            bool orderProcessingEnabled,
            bool productRecommendationsEnabled,
            bool smallTalkEnabled,
            bool autoEscalateEnabled)
        {
            SystemPrompt = systemPrompt;
            ModelName = modelName;
            ApiKey = apiKey;
            Platform = platform;
            Temperature = temperature;
            MaxTokens = maxTokens;

            OrderProcessingEnabled = orderProcessingEnabled;
            ProductRecommendationsEnabled = productRecommendationsEnabled;
            SmallTalkEnabled = smallTalkEnabled;
            AutoEscalateEnabled = autoEscalateEnabled;

            return Result.Updated;
        }

        public static Result<AIConfiguration> CreateDefault(Guid merchantId)
        {
            var id = Guid.NewGuid();

            // Realistic placeholders
            var systemPrompt = "You are a helpful AI assistant.";
            var modelName = "gpt-4o"; // default model placeholder
            var apiKey = "YOUR_API_KEY_HERE"; // placeholder to be replaced with merchant's actual key
            var platform = AIPlatform.OpenAI; // sensible default platform
            var temperature = 0.7;
            var maxTokens = 800;

            var validation = ValidateInputs(id, systemPrompt, modelName, apiKey, platform, temperature, maxTokens, merchantId);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new AIConfiguration(
                id,
                systemPrompt,
                modelName,
                apiKey,
                platform,
                temperature,
                maxTokens,
                merchantId,
                orderProcessingEnabled: true,
                productRecommendationsEnabled: true,
                smallTalkEnabled: true,
                autoEscalateEnabled: true
            );
        }

        #region Validation

        private static Result<Success> ValidateInputs(
            Guid id,
            string systemPrompt,
            string modelName,
            string apiKey,
            AIPlatform platform,
            double temperature,
            int maxTokens,
            Guid merchantId)
        {
            if (id == Guid.Empty)
                return AIConfigurationErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(systemPrompt))
                return AIConfigurationErrors.SystemPromptRequired;

            if (systemPrompt.Length > 2000)
                return AIConfigurationErrors.SystemPromptTooLong;

            if (string.IsNullOrWhiteSpace(modelName))
                return AIConfigurationErrors.ModelNameRequired;

            if (modelName.Length > 200)
                return AIConfigurationErrors.ModelNameTooLong;

            if (string.IsNullOrWhiteSpace(apiKey))
                return AIConfigurationErrors.ApiKeyRequired;

            if (apiKey.Length > 500)
                return AIConfigurationErrors.ApiKeyTooLong;

            if (!Enum.IsDefined(typeof(AIPlatform), platform))
                return AIConfigurationErrors.PlatformRequired;

            if (temperature < 0.0 || temperature > 1.0)
                return AIConfigurationErrors.TemperatureOutOfRange;

            if (maxTokens <= 0)
                return AIConfigurationErrors.MaxTokensInvalid;

            if (merchantId == Guid.Empty)
                return AIConfigurationErrors.MerchantIdRequired;

            return Result.Success;
        }

        #endregion
    }
}
