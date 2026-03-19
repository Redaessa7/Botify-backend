namespace Botify.Application.CrossCutting.Options;

public class CorsOptions
{
    public string? PolicyName { get; set; }
    public string[] AllowedOrigins { get; set; } = [];
}