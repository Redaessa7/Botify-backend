using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.ValueObjects
{
    public enum BotPlatform
    {
        TELEGRAM,
        WHATSAPP,
        MESSENGER
    }

    public static class BotPlatformExtensions
    {
        public static Result<BotPlatform> From(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
                return BotErrors.PlatformRequired;

            return platform.ToUpperInvariant() switch
            {
                "TELEGRAM" => BotPlatform.TELEGRAM,
                "WHATSAPP" => BotPlatform.WHATSAPP,
                "MESSENGER" => BotPlatform.MESSENGER,
                _ => BotErrors.PlatformInvalid
            };
        }

        public static string AsString(this BotPlatform platform) =>
            platform.ToString(); // returns uppercase string by default
    }
}
