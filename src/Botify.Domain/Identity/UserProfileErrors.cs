using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity;

public static class UserProfileErrors
{
    public static readonly Error IdentityUserIdRequired =
        Error.Validation("Profile.IdentityUserIdRequired", "IdentityUserId is required.");

    public static readonly Error FirstNameRequired =
        Error.Validation("Profile.FirstNameRequired", "First name is required.");

    public static readonly Error LastNameRequired =
        Error.Validation("Profile.LastNameRequired", "Last name is required.");
}
