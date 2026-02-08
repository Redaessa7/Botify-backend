using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Bots.ValueObjects;
using Botify.Domain.Models.Merchants.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class BotDetailsResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public BotPlatform Platform { get; init; }
        public bool IsActive { get; init; }
        public string? ApiToken { get; init; }
        public DateTime? LastPing { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        // Stats
        public TotalStatsResponse TotalStats { get; init; } = null!;
        public int UniqueCustomers { get; init; }
        public MonthlyStatsResponse? MonthlyStats { get; init; }
        public IEnumerable<BotDetailStatResponse>? WeeklyStats { get; init; }
    }

    public sealed class TotalStatsResponse
    {
        public int Messages { get; init; }
        public int Orders { get; init; }
        public SupportRequestsStatsResponse SupportRequests { get; init; } = null!;
    }

    public sealed class SupportRequestsStatsResponse
    {
        public int Total { get; init; }
        public int Pending { get; init; }
    }

    public sealed class MonthlyStatsResponse
    {
        public int Messages { get; init; }
        public int Orders { get; init; }
    }

    public sealed class BotDetailStatResponse
    {
        public string Day { get; init; } = null!;
        public int Messages { get; init; }
        public int Orders { get; init; }
    }

    public static class BotDetailsResponseExtensions
    {
        public static BotDetailsResponse ToBotDetailsResponse(this Bot bot)
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-6);

            // Weekly stats
            var weeklyStats = bot.DailyStats
                .Where(s => s.Date >= weekStart && s.Date <= today)
                .OrderBy(s => s.Date)
                .Select(s => new BotDetailStatResponse
                {
                    Day = s.Date.DayOfWeek.ToString().ToUpperInvariant(),
                    Messages = s.MessageCount,
                    Orders = s.OrderCount
                })
                .ToList();

            // Monthly stats
            var monthlyStats = new MonthlyStatsResponse
            {
                Messages = bot.DailyStats.Where(s => s.Date.Month == today.Month).Sum(s => s.MessageCount),
                Orders = bot.DailyStats.Where(s => s.Date.Month == today.Month).Sum(s => s.OrderCount)
            };

            // Total stats
            var totalStats = new TotalStatsResponse
            {
                Messages = bot.DailyStats.Sum(s => s.MessageCount),
                Orders = bot.DailyStats.Sum(s => s.OrderCount),
                SupportRequests = new SupportRequestsStatsResponse
                {
                    Total = bot.Questions.Count,
                    Pending = bot.Questions.Count(q => !q.IsResolved)
                }
            };

            return new BotDetailsResponse
            {
                Id = bot.Id,
                Name = bot.Name,
                Platform = bot.Platform,
                IsActive = bot.Active,
                ApiToken = bot.Token,
                CreatedAt = bot.CreatedAtUtc,
                UpdatedAt = bot.LastModifiedUtc,
                TotalStats = totalStats,
                UniqueCustomers = bot.Questions.Select(q => q.CustomerId).Distinct().Count(),
                MonthlyStats = monthlyStats,
                WeeklyStats = weeklyStats,
                LastPing = bot.LastPing  
            };
        }
    }
}
