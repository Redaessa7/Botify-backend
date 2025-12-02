using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Bots;

public static class BotErrors
{
    public static Error BotNotFound =>
        Error.NotFound("Bot.NotFound", "Bot not found.");

    public static Error BotPlatformConflict =>
        Error.Conflict("Bot.PlatformConflict", "Only one bot can be active per platform.");
}
