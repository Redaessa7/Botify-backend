using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Features.Merchants.Responses
{
    /// <summary>
    /// Response DTO for bots by merchant.
    /// </summary>
    public sealed class BotResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; }
        public string? ApiToken { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? LastPing { get; init; }

        public int MessagesThisMonth { get; init; }
        public int OrdersThisMonth { get; init; }
        public int QuestionsForMerchant { get; init; }

        // Factory method to map from domain Bot entity
        public static BotResponse FromDomain(Bot bot)
        {
            var today = DateTime.UtcNow.Date;

            return new BotResponse
            {
                Id = bot.Id,
                Name = bot.Name,
                Platform = bot.Platform,
                ApiToken = bot.Token,
                IsActive = bot.Active,
                CreatedAt = bot.CreatedAtUtc,
                LastPing = bot.LastPing,

                MessagesThisMonth = bot.DailyStats
                    .Where(s => s.Date.Month == today.Month && s.Date.Year == today.Year)
                    .Sum(s => s.MessageCount),

                OrdersThisMonth = bot.DailyStats
                    .Where(s => s.Date.Month == today.Month && s.Date.Year == today.Year)
                    .Sum(s => s.OrderCount),

                QuestionsForMerchant = bot.Questions.Count
            };
        }
    }
}
