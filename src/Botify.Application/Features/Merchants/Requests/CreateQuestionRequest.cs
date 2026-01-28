using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Questions;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed class CreateQuestionRequest
    {
        public Guid BotId { get; init; }
        public string BotName { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; } 
        public QuestionType Type { get; init; } 
        public string CustomerName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Telegram { get; init; }
        public string? Whatsapp { get; init; }
        public string? Messenger { get; init; }
        public string Message { get; init; } = string.Empty;

        public Result<Question> ToQuestion(Guid merchantId)
        {


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
              Platform,
              Type,
                customerResult.Value,
                Message
            );
        }
    }
}
