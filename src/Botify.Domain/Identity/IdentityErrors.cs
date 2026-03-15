using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity
{
    public static class IdentityErrors
    {
        // -----------------------
        //  User Validation
        // -----------------------
        public static Error EmailRequired =>
            Error.Validation("Identity.Email.Required", "Email is required.");
        public static Error IdRequired =>
    Error.Validation("Identity.Id.Required", "Id is required.");


        public static Error PasswordRequired =>
            Error.Validation("Identity.Password.Required", "Password is required.");

        public static Error FirstNameRequired =>
            Error.Validation("Identity.FirstName.Required", "First name is required.");

        public static Error LastNameRequired =>
            Error.Validation("Identity.LastName.Required", "Last name is required.");

        public static Error InvalidEmailFormat =>
            Error.Validation("Identity.Email.InvalidFormat", "Email format is invalid.");

        // -----------------------
        // Registration
        // -----------------------
        public static Error EmailAlreadyExists =>
            Error.Conflict("Identity.Email.Exists", "An account with this email already exists.");

        public static Error WeakPassword =>
            Error.Validation("Identity.Password.Weak", "The provided password does not meet security requirements.");

        // -----------------------
        // Login
        // -----------------------
        public static Error InvalidCredentials =>
            Error.Unauthorized("Identity.Login.InvalidCredentials", "Invalid email or password.");
        public static Error InvalidPassowrd =>
            Error.Validation("Identity.InvalidPassowrd", "Invalid password.");

        public static Error UserNotFound =>
            Error.NotFound("Identity.User.NotFound", "User not found.");

        public static Error UserLockedOut =>
            Error.Forbidden("Identity.User.LockedOut", "User is locked out due to multiple failed login attempts.");

        public static Error UserDisabled =>
            Error.Forbidden("Identity.User.Disabled", "User account is disabled.");

        // -----------------------
        // Refresh Token
        // -----------------------
        public static Error RefreshTokenNotFound =>
            Error.NotFound("Identity.RefreshToken.NotFound", "Refresh token not found.");

        public static Error RefreshTokenExpired =>
            Error.Unauthorized("Identity.RefreshToken.Expired", "Refresh token has expired.");

        public static Error RefreshTokenRevoked =>
            Error.Forbidden("Identity.RefreshToken.Revoked", "This refresh token has been revoked.");

        public static Error RefreshTokenInvalid =>
            Error.Unauthorized("Identity.RefreshToken.Invalid", "Invalid refresh token.");

        // -----------------------
        // Roles / Permissions
        // -----------------------
        public static Error RoleNotFound =>
            Error.NotFound("Identity.Role.NotFound", "Role not found.");

        public static Error UserDoesNotHaveRole =>
            Error.Forbidden("Identity.Role.UserMissing", "User does not have the required role.");

        public static Error CannotAssignRole =>
            Error.Conflict("Identity.Role.AssignFailed", "Failed to assign role to user.");

        // -----------------------
        // Token Generation Errors
        // -----------------------
        public static Error TokenGenerationFailed =>
            Error.Failure("Identity.Token.Failed", "Failed to generate authentication token.");

        public static Error InvalidJwtConfiguration =>
            Error.Failure("Identity.JWT.ConfigInvalid", "JWT configuration is invalid.");

        // -----------------------
        // Logout / Revoke
        // -----------------------
        public static Error LogoutFailed =>
            Error.Failure("Identity.Logout.Failed", "Failed to logout or revoke refresh token.");
    }
}
