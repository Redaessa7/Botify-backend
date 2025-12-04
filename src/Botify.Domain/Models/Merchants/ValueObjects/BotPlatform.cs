using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.ValueObjects;

public record BotPlatform(string Name)
{
    public static BotPlatform Telegram => new("telegram");
    public static BotPlatform WhatsApp => new("whatsApp");
    public static BotPlatform Messenger => new("messenger");

    public override string ToString() => Name;

    public static Result<BotPlatform> From(string platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
            return BotErrors.PlatformRequired;

        return platform.ToLowerInvariant() switch
        {
            "telegram" => Telegram,
            "whatsapp" => WhatsApp,
            "messenger" => Messenger,
            _ => BotErrors.PlatformInvalid
        };
    }
}
