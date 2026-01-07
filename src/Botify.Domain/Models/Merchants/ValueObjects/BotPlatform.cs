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
        /// <summary>
        /// Convert the enum value into a consistent uppercase string key.
        /// </summary>
        public static string AsKey(this BotPlatform platform) =>
            platform.ToString().ToUpperInvariant();
    }
}
