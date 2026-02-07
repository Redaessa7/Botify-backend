using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants;

public static class MerchantErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("Merchant.General.Id.Required", "Merchant ID is required.");

    public static Error MerchantNameRequired =>
        Error.Validation("Merchant.General.Name.Required", "Merchant name is required.");
    // Preferences
    public static Error PreferencesUpdateFailed =>
        Error.Validation("Merchant.Preferences.UpdateFailed", "Unable to update merchant preferences.");

    public static Error MerchantNameTooLong =>
        Error.Validation("Merchant.General.Name.TooLong", "Merchant name cannot exceed 100 characters.");

    public static Error IndustryRequired =>
        Error.Validation("Merchant.General.Industry.Required", "Industry is required.");

    public static Error IndustryTooLong =>
        Error.Validation("Merchant.General.Industry.TooLong", "Industry cannot exceed 100 characters.");

    public static Error LogoUrlRequired =>
        Error.Validation("Merchant.General.LogoUrl.Required", "Logo URL is required.");

    public static Error LogoUrlInvalid =>
        Error.Validation("Merchant.General.LogoUrl.Invalid", "Logo URL must be a valid URL.");

    public static Error UserProfileRequired =>
        Error.Validation("Merchant.General.UserProfile.Required", "User profile is required.");

    // Status
    public static Error ShopAlreadyAssigned =>
    Error.Validation("Merchant.Shop.AlreadyAssigned", "Shop is already assigned to this merchant.");
    public static Error NotPending =>
        Error.Conflict("Merchant.Status.NotPending", "Merchant must be pending to approve.");

    public static Error NotApproved =>
        Error.Conflict("Merchant.Status.NotApproved", "Merchant must be approved before this action.");

    public static Error Suspended =>
        Error.Forbidden("Merchant.Status.Suspended", "Merchant is suspended and cannot perform this action.");

    // Subscription
    public static Error SubscriptionExpired =>
        Error.Unauthorized("Merchant.Subscription.Expired", "Subscription expired. Please renew.");

    public static Error PlanLimitReached =>
        Error.Conflict("Merchant.Subscription.PlanLimitReached", "Plan limit reached. Upgrade required.");

    // Lookup
    public static Error NotFound =>
        Error.NotFound("Merchant.Lookup.NotFound", "The specified merchant could not be found.");
}
