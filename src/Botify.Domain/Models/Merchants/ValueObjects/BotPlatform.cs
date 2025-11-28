
namespace Botify.Domain.Models.Merchants.ValueObjects;
// BotPlatform
public record BotPlatform(string Name)
{
    public static BotPlatform Telegram => new("Telegram");
    public static BotPlatform WhatsApp => new("WhatsApp");
    public static BotPlatform Messenger => new("Messenger");

    public override string ToString() => Name;
}
