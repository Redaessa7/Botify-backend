namespace Botify.Application.Features.Auths.Requests;

public sealed record GenerateEmailChangeOtpRequest(string OldEmail, string NewEmail, string CurrentPassword);
