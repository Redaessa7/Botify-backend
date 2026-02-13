namespace Botify.Application.Features.Auths.Requests;

public sealed record ResetPasswordRequest(
    string? Email,
    string? ResetToken,
    string NewPassword,
    string ConfirmPassword
);
