namespace Botify.Application.CrossCutting.Requests;

public class LogoutRequest
{
    public string RefreshToken { get; set; } = default!;
}
