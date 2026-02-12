namespace Botify.Application.Features.Auths.Requests;

public sealed record ChangePasswordRequest(
    string Email,
    string OldPassword,
    string NewPassword,
    string ConfirmPassword
);
