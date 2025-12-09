using Botify.Domain.Models.Merchants.Bots;

namespace Botify.Application.Services.Merchants.Responses
{
    /// <summary>
    /// Response DTO for  bots by merchant.
    /// </summary>
    public sealed class BotResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public string? WebhookUrl { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateOnly LastPing { get; init; }

        public int MessagesThisMonth { get; init; } = 0;
        public int OrdersThisMonth { get; init; } = 0;
        public int QuestionsForMerchant { get; init; } = 0;

        // Factory method to map from domain Bot entity
        public static BotResponse FromDomain(Bot bot) => new()
        {
            Id = bot.Id,
            Name = bot.Name,
            Platform = bot.Platform.ToString(),
            WebhookUrl = bot.WebhookUrl,
            IsActive = bot.Active,
            CreatedAt = bot.CreatedAtUtc,
            LastPing = DateOnly.FromDateTime(DateTime.UtcNow),
            MessagesThisMonth = 0,
            OrdersThisMonth = 0,
            QuestionsForMerchant = 0
        };
    }
}
