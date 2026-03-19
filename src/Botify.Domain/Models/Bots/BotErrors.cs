using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Bots;

public static class BotErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("Bot.Id.Required", "Bot ID is required.");

    public static Error MerchantIdRequired =>
        Error.Validation("Bot.MerchantId.Required", "Merchant ID is required.");

    public static Error SendMessageFailed =>
        Error.Conflict("Bot.Message.SendFailed", "Failed to send message to the bot platform.");

    public static Error NameRequired =>
        Error.Validation("Bot.Name.Required", "Bot name is required.");

    public static Error NameTooLong =>
        Error.Validation("Bot.Name.TooLong", "Bot name cannot exceed 100 characters.");

    public static Error TokenRequired =>
        Error.Validation("Bot.Token.Required", "Bot token is required.");

    public static Error TokenInvalid =>
        Error.Validation("Bot.Token.Invalid", "Bot token is invalid or unauthorized.");

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

    // Status / Lifecycle
    public static Error PlatformNotVerified =>
        Error.Conflict("Bot.Platform.NotVerified", "Platform must be verified before activating bot.");

    public static Error AlreadyActive =>
        Error.Conflict("Bot.Status.AlreadyActive", "Bot is already active.");

    public static Error AlreadyInactive =>
        Error.Conflict("Bot.Status.AlreadyInactive", "Bot is already inactive.");

    public static Error BotPlatformConflict =>
        Error.Conflict("Bot.Platform.Conflict", "Only one bot can be active per platform.");

    public static Error WebhookRegistrationFailed =>
        Error.Conflict("Bot.Webhook.RegistrationFailed", "Failed to register webhook with external platform.");

    public static Error WebhookNotSet =>
        Error.Conflict("Bot.Webhook.NotSet", "Webhook must be set before receiving updates.");

    public static Error WebhookRemovalFailed =>
        Error.Conflict("Bot.Webhook.RemovalFailed", "Failed to remove webhook from external platform.");

    public static Error WebhookRefreshFailed =>
        Error.Conflict("Bot.Webhook.RefreshFailed", "Failed to refresh webhook with new configuration.");

    // Lookup
    public static Error BotNotFound =>
        Error.NotFound("Bot.NotFound", "Bot not found.");

    public static Error MerchantNotFound =>
        Error.NotFound("Bot.Merchant.NotFound", "Merchant not found.");

    // Permissions / Security
    public static Error UnauthorizedAccess =>
        Error.Unauthorized("Bot.Unauthorized", "Unauthorized access to bot.");

    public static Error SecretMismatch =>
        Error.Unauthorized("Bot.Secret.Mismatch", "Provided secret does not match bot secret.");
}
