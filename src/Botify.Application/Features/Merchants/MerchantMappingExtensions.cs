using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;

namespace Botify.Application.Services.Merchants
{
    public static class MerchantMappingExtensions
    {
        public static CurrentMerchantResponse ToCurrentResponse(this Merchant merchant)
        {
            return new CurrentMerchantResponse
            {
                merchantId = merchant.Id,
                MerchantName = merchant.MerchantName,
                LogoUrl = merchant.LogoUrl,
                Status = merchant.Status,
                VerifiedAt = merchant.VerifiedAt,

                FirstName = merchant.Profile.FirstName,
                LastName = merchant.Profile.LastName,
                Email = merchant.Profile.IdentityUserId, // or actual email if stored separately
                ProfileImageUrl = merchant.Profile.ProfileImageUrl,
                Bio = merchant.Profile.Bio,
                Language = merchant.Profile.Language,
                TimeZone = merchant.Profile.TimeZone,

                CurrentPlan = merchant.Subscription.CurrentPlan,
                ExpiryDate = merchant.Subscription.ExpiryDate
            };
        }
    }
}
