namespace Botify.Application.Requests;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = default!;
}
