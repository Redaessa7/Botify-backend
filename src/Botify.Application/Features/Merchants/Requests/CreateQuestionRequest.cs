using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed class CreateQuestionRequest
    {
        public Guid BotId { get; init; }
        public string BotName { get; init; } = string.Empty;
        public string Platform { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Telegram { get; init; }
        public string? Whatsapp { get; init; }
        public string? Messenger { get; init; }
        public string Message { get; init; } = string.Empty;

        public Result<Question> ToQuestion(Guid merchantId)
        {
            // Convert platform string to enum
            var platformResult = BotPlatformExtensions.From(Platform);
            if (!platformResult.IsSuccess)
                return platformResult.Errors;

            // Convert type string to enum
            var typeResult = QuestionTypeExtensions.From(Type);
            if (!typeResult.IsSuccess)
                return typeResult.Errors;

            // Build CustomerInfo value object
            var customerResult = CustomerInfo.Create(CustomerName, Email, Telegram, Whatsapp, Messenger);
            if (!customerResult.IsSuccess)
                return customerResult.Errors;

            // Create Question entity
            return Question.Create(
                Guid.NewGuid(),
                merchantId,
                BotId,
                BotName,
                platformResult.Value,
                typeResult.Value,
                customerResult.Value,
                Message
            );
        }
    }
}
