using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class SupportRequestDto
    {
        public Guid Id { get; init; }
        public Guid MerchantId { get; init; }
        public Guid BotId { get; init; }
        public string BotName { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; }
        public SupportRequestType Type { get; init; }
        public RequestReason Reason { get; init; }

        // ✅ Contact info grouped
        public ContactInfoResponse ContactInfo { get; init; } = null!;

        public string Message { get; init; } = string.Empty;
        public bool IsResolved { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ResolvedAt { get; init; }

        public static SupportRequestDto FromDomain(SupportRequest supportRequest) =>
            new()
            {
                Id = supportRequest.Id,
                MerchantId = supportRequest.MerchantId,
                BotId = supportRequest.BotId,
                BotName = supportRequest.BotName,
                Platform = supportRequest.Platform,
                Type = supportRequest.Type,
                Reason = supportRequest.Reason,
                ContactInfo = new ContactInfoResponse
                {
                    CustomerName = supportRequest.CustomerName,
                    PlatformContact = supportRequest.PlatformContact
                },
                Message = supportRequest.Message,
                IsResolved = supportRequest.IsResolved,
                CreatedAt = supportRequest.CreatedAtUtc,
                ResolvedAt = supportRequest.ResolvedAt
            };
    }
}


