using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;

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
            Guid merchantId) : base(id)
        {
            SystemPrompt = systemPrompt;
            ModelName = modelName;
            ApiKey = apiKey;
            Platform = platform;
            Temperature = temperature;
            MaxTokens = maxTokens;
            MerchantId = merchantId;
        }

        public static Result<AIConfiguration> Create(
            Guid id,
            string systemPrompt,
            string modelName,
            string apiKey,
            AIPlatform platform,
            double temperature,
            int maxTokens,
            Guid merchantId)
        {
            var validation = ValidateInputs(id, systemPrompt, modelName, apiKey, platform, temperature, maxTokens, merchantId);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new AIConfiguration(id, systemPrompt, modelName, apiKey, platform, temperature, maxTokens, merchantId);
        }
        public Result<Updated> Update(string systemPrompt, string modelName, string apiKey, AIPlatform platform, double temperature, int maxTokens)
        {
            SystemPrompt = systemPrompt;
            ModelName = modelName;
            ApiKey = apiKey;
            Platform = platform;
            Temperature = temperature;
            MaxTokens = maxTokens;

            return Result.Updated;
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
