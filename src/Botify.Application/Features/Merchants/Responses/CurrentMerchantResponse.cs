using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class SettingsResponse
    {
        public bool HasAIConfig { get; init; }
        public bool HasShop { get; init; }
    }
    public sealed class CurrentMerchantResponse
    {
        public Guid merchantId { get; init; }
        public MerchantStatus Status { get; init; }
        public DateTime? VerifiedAt { get; init; }

        public int Preferences { get; init; }
        // Profile info
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? ProfileImageUrl { get; init; }
        public string? Bio { get; init; }
        public string? Language { get; init; }
        public SettingsResponse Settings { get; init; } = null!;
        // Subscription info
        public Plan CurrentPlan { get; init; }
        public DateTime ExpiryDate { get; init; }
    }
    public static class CurrentMerchantResponseExtensions
    {
        public static CurrentMerchantResponse ToCurrentResponse(this Merchant merchant)
        {
            return new CurrentMerchantResponse
            {
                merchantId = merchant.Id,
                Status = merchant.Status,
                VerifiedAt = merchant.VerifiedAt,

                FirstName = merchant.Profile.FirstName,
                LastName = merchant.Profile.LastName,
                Email = merchant.Profile.Email,
                ProfileImageUrl = merchant.Profile.ProfileImageUrl,
                Bio = merchant.Profile.Bio,
                Language = merchant.Profile.Language,
                Settings = new SettingsResponse
                {
                    HasAIConfig = merchant.AIConfiguration != null,
                    HasShop = merchant.ShopSettings != null
                },
                CurrentPlan = merchant.Subscription.CurrentPlan,
                ExpiryDate = merchant.Subscription.ExpiryDate,
                Preferences = (int)merchant.Preferences
            };
        }
    }
}