namespace Botify.Application.Requests
{
    /// <summary>
    /// Request payload for checking whether a bot token is valid.
    /// </summary>
    public sealed record CheckTokenRequest(
        string Platform,
        string ApiToken
    );
}
