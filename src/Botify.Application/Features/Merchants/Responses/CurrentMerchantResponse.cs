using Botify.Domain.Models.Merchants.Enums;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class CurrentMerchantResponse
    {
        public Guid merchantId { get; init; }
        public string MerchantName { get; init; } = null!;
        public string? LogoUrl { get; init; }
        public MerchantStatus Status { get; init; }
        public DateTime? VerifiedAt { get; init; }

        // Profile info
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;
        public string Email { get; init; } = null!;
        public string? ProfileImageUrl { get; init; }
        public string? Bio { get; init; }
        public string? Language { get; init; }
        public string? TimeZone { get; init; }

        // Subscription info
        public Plan CurrentPlan { get; init; }
        public DateTime ExpiryDate { get; init; }
    }
}
