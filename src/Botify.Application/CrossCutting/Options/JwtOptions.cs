namespace Botify.Application.CrossCutting.Options;

public class JwtOptions
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int TokenExpirationInMinutes { get; set; }
    public int RefreshTokenExpirationInDays { get; set; }
    public string? SecurityKey { get; set; }
}
