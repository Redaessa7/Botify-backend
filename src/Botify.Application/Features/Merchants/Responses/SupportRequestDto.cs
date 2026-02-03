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
        public ContactInfoResponse ContactInfo { get; init; } = null!;
        public string Message { get; init; } = string.Empty;
        public bool IsResolved { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ResolvedAt { get; init; }

        public static SupportRequestDto FromDomain(SupportRequest question) =>
            new()
            {
                Id = question.Id,
                MerchantId = question.MerchantId,
                BotId = question.BotId,
                BotName = question.BotName,
                Platform = question.Platform,
                Type = question.Type,
                ContactInfo = ContactInfoResponse.FromDomain(question.Customer),
                Message = question.Message,
                IsResolved = question.IsResolved,
                CreatedAt = question.CreatedAtUtc,
                Reason = question.Reason,
                ResolvedAt = question.ResolvedAt
            };
    }
}
