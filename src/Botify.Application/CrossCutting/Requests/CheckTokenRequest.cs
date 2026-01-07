using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.CrossCutting.Requests
{
    /// <summary>
    /// Request payload for checking whether a bot token is valid.
    /// </summary>
    public sealed record CheckTokenRequest(
        BotPlatform BotPlatform,
        string ApiToken
    );
}
