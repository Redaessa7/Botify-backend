using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Bots;

public static class BotErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("Bot.Id.Required", "Bot ID is required.");

    public static Error MerchantIdRequired =>
        Error.Validation("Bot.MerchantId.Required", "Merchant ID is required.");

    public static Error NameRequired =>
        Error.Validation("Bot.Name.Required", "Bot name is required.");

    public static Error NameTooLong =>
        Error.Validation("Bot.Name.TooLong", "Bot name cannot exceed 100 characters.");

    public static Error TokenRequired =>
        Error.Validation("Bot.Token.Required", "Bot token is required.");

    public static Error SecretRequired =>
        Error.Validation("Bot.Secret.Required", "Bot secret is required.");

    public static Error PlatformRequired =>
        Error.Validation("Bot.Platform.Required", "Bot platform is required.");

    public static Error PlatformInvalid =>
        Error.Validation("Bot.Platform.Invalid", "Unsupported bot platform.");

    public static Error WebhookUrlRequired =>
        Error.Validation("Bot.WebhookUrl.Required", "Webhook URL is required.");

    public static Error WebhookUrlInvalid =>
        Error.Validation("Bot.WebhookUrl.Invalid", "Webhook URL must be a valid URL.");

    // Status
    public static Error PlatformNotVerified =>
        Error.Conflict("Bot.Platform.NotVerified", "Platform must be verified before enabling bot.");

    public static Error AlreadyEnabled =>
        Error.Conflict("Bot.Status.AlreadyEnabled", "Bot is already enabled.");

    public static Error AlreadyDisabled =>
        Error.Conflict("Bot.Status.AlreadyDisabled", "Bot is already disabled.");

    public static Error BotPlatformConflict =>
        Error.Conflict("Bot.Platform.Conflict", "Only one bot can be active per platform.");

    // Lookup
    public static Error BotNotFound =>
        Error.NotFound("Bot.NotFound", "Bot not found.");
}
