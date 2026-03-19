namespace Botify.Domain.Models.Merchants.ValueObjects;

public record SocialMediaLinks
{
    public string? Whatsapp { get; init; }
    public string? Website { get; init; }
    public string? Facebook { get; init; }
    public string? Twitter { get; init; }
    public string? Instagram { get; init; }
    public string? LinkedIn { get; init; }
};
