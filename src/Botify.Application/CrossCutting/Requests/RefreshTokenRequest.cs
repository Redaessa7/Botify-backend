namespace Botify.Application.CrossCutting.Requests;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}
