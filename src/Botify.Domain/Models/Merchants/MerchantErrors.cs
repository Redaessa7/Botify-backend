using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public static class MerchantErrors
    {
        // Validation
        public static Error FullNameRequired =>
            Error.Validation("Merchant.FullName.Required", "Full name is required.");

        public static Error ImageUrlRequired =>
            Error.Validation("Merchant.ImageUrl.Required", "Image URL is required.");

        // Status
        public static Error NotPending =>
            Error.Conflict("Merchant.NotPending", "Merchant must be pending to approve.");

        public static Error NotApproved =>
            Error.Conflict("Merchant.NotApproved", "Merchant must be approved before this action.");

        public static Error Suspended =>
            Error.Forbidden("Merchant.Suspended", "Merchant is suspended and cannot perform this action.");

        // Shop
        public static Error ShopIncomplete =>
            Error.Validation("Shop.Incomplete", "Shop details must be complete before adding or activating bots.");

        // Subscription
        public static Error SubscriptionExpired =>
            Error.Unauthorized("Subscription.Expired", "Subscription expired. Please renew.");

        public static Error PlanLimitReached =>
            Error.Conflict("Plan.LimitReached", "Plan limit reached. Upgrade required.");
        public static readonly Error NotFound =
    Error.NotFound("Merchant.NotFound", "The specified merchant could not be found.");

        // Bot
        public static Error BotNotFound =>
            Error.NotFound("Bot.NotFound", "Bot not found.");

        public static Error BotPlatformConflict =>
            Error.Conflict("Bot.PlatformConflict", "Only one bot can be active per platform.");
    }
}
