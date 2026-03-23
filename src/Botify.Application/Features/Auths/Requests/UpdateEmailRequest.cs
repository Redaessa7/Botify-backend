namespace Botify.Application.Features.Auths.Requests;

public sealed record UpdateEmailRequest(
    string NewEmail,
    string CurrentPassword,
    string? OldEmail,
    string? ResetToken
);
