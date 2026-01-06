using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.AI;

public static class AIConfigurationErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("AIConfiguration.General.Id.Required", "AI Configuration ID is required.");

    public static Error ModelNameRequired =>
        Error.Validation("AIConfiguration.General.ModelName.Required", "Model name is required.");

    public static Error ModelNameTooLong =>
        Error.Validation("AIConfiguration.General.ModelName.TooLong", "Model name cannot exceed 200 characters.");

    public static Error ApiKeyRequired =>
        Error.Validation("AIConfiguration.General.ApiKey.Required", "API key is required.");

    public static Error ApiKeyTooLong =>
        Error.Validation("AIConfiguration.General.ApiKey.TooLong", "API key cannot exceed 500 characters.");

    public static Error PlatformRequired =>
        Error.Validation("AIConfiguration.General.Platform.Required", "AI platform is required.");

    public static Error TemperatureOutOfRange =>
        Error.Validation("AIConfiguration.General.Temperature.OutOfRange", "Temperature must be between 0.0 and 1.0.");

    public static Error MaxTokensInvalid =>
        Error.Validation("AIConfiguration.General.MaxTokens.Invalid", "Max tokens must be greater than zero.");

    public static Error MerchantIdRequired =>
        Error.Validation("AIConfiguration.General.MerchantId.Required", "Merchant ID is required.");

    // Status / Lookup
    public static Error NotFound =>
        Error.NotFound("AIConfiguration.Lookup.NotFound", "The specified AI configuration could not be found.");
    public static Error SystemPromptRequired =>
    Error.Validation("AIConfiguration.General.SystemPrompt.Required", "System prompt is required.");

    public static Error SystemPromptTooLong =>
        Error.Validation("AIConfiguration.General.SystemPrompt.TooLong", "System prompt cannot exceed 2000 characters.");

    public static Error AlreadyExists =>
        Error.Conflict("AIConfiguration.Lookup.AlreadyExists", "An AI configuration already exists for this merchant.");
}
