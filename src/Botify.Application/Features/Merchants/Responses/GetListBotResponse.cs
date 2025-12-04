using Botify.Domain.Models.Merchants.Bots;

namespace Botify.Application.Services.Merchants.Responses
{
    /// <summary>
    /// Response DTO for listing bots by merchant.
    /// </summary>
    public sealed class GetListBotResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public string? WebhookUrl { get; init; }
        public bool IsActive { get; init; }

        // Factory method to map from domain Bot entity
        public static GetListBotResponse FromDomain(Bot bot) => new()
        {
            Id = bot.Id,
            Name = bot.Name,
            Platform = bot.Platform.Name,
            WebhookUrl = bot.WebhookUrl,
            IsActive = bot.Enabled
        };
    }
}
