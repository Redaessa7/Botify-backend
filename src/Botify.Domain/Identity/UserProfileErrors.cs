using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity;

public static class UserProfileErrors
{
    // Identity
    public static readonly Error IdentityUserIdRequired =
        Error.Validation("Profile.IdentityUserId.Required", "IdentityUserId is required.");

    public static readonly Error EmailRequired =
        Error.Validation("Profile.Email.Required", "Email is required.");

    public static readonly Error EmailInvalid =
        Error.Validation("Profile.Email.Invalid", "Invalid email format.");

    public static readonly Error PasswordRequired =
        Error.Validation("Profile.Password.Required", "Password is required.");

    public static readonly Error PasswordTooShort =
        Error.Validation("Profile.Password.TooShort", "Password must be at least 6 characters.");

    public static readonly Error PhoneRequired =
        Error.Validation("Profile.Phone.Required", "Phone number is required.");

    public static readonly Error PhoneInvalid =
        Error.Validation("Profile.Phone.Invalid", "Invalid phone number format.");

    // Personal Info
    public static readonly Error FirstNameRequired =
        Error.Validation("Profile.FirstName.Required", "First name is required.");

    public static readonly Error FirstNameLengthInvalid =
        Error.Validation("Profile.FirstName.LengthInvalid", "First name must be between 2 and 50 characters.");

    public static readonly Error LastNameRequired =
        Error.Validation("Profile.LastName.Required", "Last name is required.");

    public static readonly Error LastNameLengthInvalid =
        Error.Validation("Profile.LastName.LengthInvalid", "Last name must be between 2 and 50 characters.");

    public static readonly Error LanguageTooLong =
        Error.Validation("Profile.Language.TooLong", "Language code cannot exceed 5 characters.");

    // Media
    public static readonly Error ProfileImageUrlInvalid =
        Error.Validation("Profile.ImageUrl.Invalid", "ProfileImageUrl must be a valid URL.");
}
