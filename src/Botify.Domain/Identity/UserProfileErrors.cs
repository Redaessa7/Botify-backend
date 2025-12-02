using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity;

public static class UserProfileErrors
{
    // Identity
    public static readonly Error IdentityUserIdRequired =
        Error.Validation("Profile.IdentityUserIdRequired", "IdentityUserId is required.");

    public static readonly Error EmailRequired =
        Error.Validation("Profile.EmailRequired", "Email is required.");

    public static readonly Error EmailInvalid =
        Error.Validation("Profile.EmailInvalid", "Invalid email format.");

    public static readonly Error PasswordRequired =
        Error.Validation("Profile.PasswordRequired", "Password is required.");

    public static readonly Error PasswordTooShort =
        Error.Validation("Profile.PasswordTooShort", "Password must be at least 6 characters.");

    public static readonly Error PhoneRequired =
        Error.Validation("Profile.PhoneRequired", "Phone number is required.");

    public static readonly Error PhoneInvalid =
        Error.Validation("Profile.PhoneInvalid", "Invalid phone number format.");

    // Profile
    public static readonly Error FirstNameRequired =
        Error.Validation("Profile.FirstNameRequired", "First name is required.");

    public static readonly Error FirstNameLengthInvalid =
        Error.Validation("Profile.FirstNameLengthInvalid", "First name must be between 2 and 50 characters.");

    public static readonly Error LastNameRequired =
        Error.Validation("Profile.LastNameRequired", "Last name is required.");

    public static readonly Error LastNameLengthInvalid =
        Error.Validation("Profile.LastNameLengthInvalid", "Last name must be between 2 and 50 characters.");

    public static readonly Error LanguageTooLong =
        Error.Validation("Profile.LanguageTooLong", "Language code cannot exceed 5 characters.");

    // Merchant
    public static readonly Error MerchantNameRequired =
        Error.Validation("Profile.MerchantNameRequired", "Merchant name is required.");

    public static readonly Error MerchantNameTooLong =
        Error.Validation("Profile.MerchantNameTooLong", "Merchant name cannot exceed 100 characters.");

    public static readonly Error IndustryRequired =
        Error.Validation("Profile.IndustryRequired", "Industry is required.");

    public static readonly Error IndustryTooLong =
        Error.Validation("Profile.IndustryTooLong", "Industry cannot exceed 100 characters.");

    public static readonly Error LogoUrlInvalid =
        Error.Validation("Profile.LogoUrlInvalid", "LogoUrl must be a valid URL.");

    public static readonly Error ProfileImageUrlInvalid =
        Error.Validation("Profile.ProfileImageUrlInvalid", "ProfileImageUrl must be a valid URL.");
}
