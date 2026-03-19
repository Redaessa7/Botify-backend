using Botify.Domain.Models.Bots;

namespace Botify.Application.Features.Merchants.Responses
{
    /// <summary>
    /// Response DTO for listing bots by merchant.
    /// </summary>
    public sealed class GetListBotResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public string ApiToken { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? LastPing { get; init; }

        public int MessagesThisMonth { get; init; }
        public int OrdersThisMonth { get; init; }
        public int QuestionsForMerchant { get; init; }

        // Factory method to map from domain Bot entity
        public static GetListBotResponse FromDomain(Bot bot)
        {
            var today = DateTime.UtcNow.Date;

            return new GetListBotResponse
            {
                Id = bot.Id,
                Name = bot.Name,
                Platform = bot.Platform.ToString(),
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
