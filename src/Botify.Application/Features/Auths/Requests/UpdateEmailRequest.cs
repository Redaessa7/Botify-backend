namespace Botify.Application.Features.Auths.Requests;

public sealed record UpdateEmailRequest(
    string OldEmail,
    string CurrentPassword,
    string? NewEmail,
    string? ResetToken
);
