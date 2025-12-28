using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class QuestionResponse
    {
        public Guid Id { get; init; }
        public Guid MerchantId { get; init; }
        public Guid BotId { get; init; }
        public string BotName { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public ContactInfoResponse ContactInfo { get; init; } = null!;
        public string Message { get; init; } = string.Empty;
        public bool IsResolved { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ResolvedAt { get; init; }

        public static QuestionResponse FromDomain(Question question) =>
            new()
            {
                Id = question.Id,
                MerchantId = question.MerchantId,
                BotId = question.BotId,
                BotName = question.BotName,
                Platform = question.Platform.AsString(),
                Type = question.Type.AsString(),
                ContactInfo = ContactInfoResponse.FromDomain(question.Customer),
                Message = question.Message,
                IsResolved = question.IsResolved,
                CreatedAt = question.CreatedAtUtc,
                ResolvedAt = question.ResolvedAt
            };
    }
}
