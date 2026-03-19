using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class HomePageInitDataResponse
    {
        public MerchantProfileResponse MerchantInfo { get; init; } = null!;
        public PlanResponse Plan { get; init; } = null!;
        public QuickActionsResponse QuickActions { get; init; } = null!;
        public SettingsProcessResponse SettingsProcess { get; init; } = null!;
        public StatsResponse Stats { get; init; } = null!;
        public ShopInfoResponse ShopInfo { get; init; } = null!;
        public IEnumerable<BotHomeResponse> BotsInfo { get; init; } = [];
        public ServiceStatusResponse ServiceStatus { get; init; } = null!;
        public MetadataResponse Metadata { get; init; } = null!;
    }

    public sealed class MerchantProfileResponse
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public sealed class PlanResponse
    {
        public Plan Type { get; init; }
        public DateTime? ExpiryDate { get; init; }
    }

    public sealed class QuickActionsResponse
    {
        public int QuickActionsSum { get; init; }
        public IEnumerable<string> SelectedActions { get; init; } = [];
    }

    public sealed class SettingsProcessResponse
    {
        public int StepNum { get; init; }
        public bool HasAiConfigured { get; init; }
        public bool HasShopConfigured { get; init; }
        public bool HasBot { get; init; }
        public bool HasProducts { get; init; }
    }

    public sealed class StatsResponse
    {
        public decimal Revenue { get; init; }
        public int Orders { get; init; }
        public int Messages { get; init; }
        public int Customers { get; init; }
    }

    public sealed class ShopInfoResponse
    {
        public string Name { get; init; } = string.Empty;
        public string? LogoUrl { get; init; }
    }

    public sealed class BotHomeResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public int TodayMessagesCount { get; init; }
    }

    public sealed class ServiceStatusResponse
    {
        public bool ServiceActivated { get; init; }
        public int NotificationsCount { get; init; }
    }

    public sealed class MetadataResponse
    {
        public DateTime LastUpdated { get; init; }
    }

    public static class HomePageInitDataResponseExtensions
    {
        public static HomePageInitDataResponse ToHomePageInitDataResponse(this Merchant merchant)
        {
            return new HomePageInitDataResponse
            {
                MerchantInfo = new MerchantProfileResponse
                {
                    FullName = $"{merchant.Profile.FirstName} {merchant.Profile.LastName}",
                    Email = merchant.Profile.Email
                },
                Plan = new PlanResponse
                {
                    Type = merchant.Subscription.CurrentPlan,
                    ExpiryDate = merchant.Subscription.ExpiryDate
                },
                QuickActions = new QuickActionsResponse
                {
                    QuickActionsSum = (int)merchant.Preferences,
                    SelectedActions = Enum.GetValues(typeof(QuickActions))
                        .Cast<QuickActions>()
                        .Where(a => a != QuickActions.None && merchant.Preferences.HasFlag(a))
                        .Take(4) // enforce max of 4
                        .Select(a => a.ToString())
                },
                SettingsProcess = new SettingsProcessResponse
                {
                    StepNum = merchant.GetOnboardingStep(),
                    HasAiConfigured = merchant.AIConfiguration != null,
                    HasShopConfigured = merchant.ShopSettings != null,
                    HasBot = merchant.Bots.Any(),
                    HasProducts = merchant.Products.Any()
                },
                Stats = new StatsResponse
                {
                    Revenue = merchant.Orders.Sum(x => x.TotalAmount),
                    Orders = merchant.Orders.Count,
                    Messages = merchant.Bots
                        .SelectMany(b => b.DailyStats)
                        .Sum(ds => ds.MessageCount),
                    Customers = merchant.Customers.Count
                },
                ShopInfo = new ShopInfoResponse
                {
                    Name = merchant.ShopSettings?.Name ?? string.Empty,
                    LogoUrl = merchant.ShopSettings?.LogoImageUrl ?? string.Empty
                },
                BotsInfo = merchant.Bots.Select(b => new BotHomeResponse
                {
                    Id = b.Id,
                    Name = b.Name,
                    Platform = b.Platform.ToString(),
                    TodayMessagesCount = b.DailyStats
                        .Where(s => s.Date == DateTime.UtcNow.Date)
                        .Sum(s => s.MessageCount)
                }),
                ServiceStatus = new ServiceStatusResponse
                {
                    ServiceActivated = merchant.IsServiceActivated,
                    NotificationsCount = 0
                },
                Metadata = new MetadataResponse
                {
                    LastUpdated = merchant.LastUpdated
                }
            };
        }
    }
}
