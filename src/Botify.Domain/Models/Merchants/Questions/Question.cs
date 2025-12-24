using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using System;

namespace Botify.Domain.Models.Merchants.Questions
{
    public sealed class Question : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public Guid BotId { get; private set; }
        public string BotName { get; private set; } = string.Empty;
        public BotPlatform Platform { get; private set; }
        public QuestionType Type { get; private set; }
        public CustomerInfo Customer { get; private set; }   // ✅ unified customer info
        public string Message { get; private set; } = string.Empty;
        public bool IsResolved { get; private set; } = false;
        public DateTime? ResolvedAt { get; private set; }

        private Question() { }

        private Question(
            Guid id,
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            QuestionType type,
            CustomerInfo customer,
            string message)
            : base(id)
        {
            MerchantId = merchantId;
            BotId = botId;
            BotName = botName;
            Platform = platform;
            Type = type;
            Customer = customer;
            Message = message;
            IsResolved = false;
            ResolvedAt = null;
        }

        #region Factory

        public static Result<Question> Create(
            Guid id,
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            QuestionType type,
            CustomerInfo customer,
            string message)
        {
            var validationResult = ValidateInputs(id, merchantId, botId, botName, platform, type, customer, message);
            if (!validationResult.IsSuccess)
                return validationResult.Errors;

            var question = new Question(
                id,
                merchantId,
                botId,
                botName,
                platform,
                type,
                customer,
                message);

            return question;
        }

        #endregion

        #region Domain Operations

        public Result<Success> Resolve()
        {
            if (IsResolved)
                return QuestionErrors.AlreadyResolved;

            IsResolved = true;
            ResolvedAt = DateTime.UtcNow;
            return Result.Success;
        }

        public Result<Success> Unresolve()
        {
            if (!IsResolved)
                return QuestionErrors.NotResolved;

            IsResolved = false;
            ResolvedAt = null;
            return Result.Success;
        }

        #endregion

        #region Validation

        private static Result<Success> ValidateInputs(
            Guid id,
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            QuestionType type,
            CustomerInfo customer,
            string message)
        {
            if (id == Guid.Empty)
                return QuestionErrors.IdRequired;

            if (merchantId == Guid.Empty)
                return QuestionErrors.MerchantIdRequired;

            if (botId == Guid.Empty)
                return QuestionErrors.BotIdRequired;

            if (string.IsNullOrWhiteSpace(botName))
                return QuestionErrors.BotNameRequired;

            if (!Enum.IsDefined(typeof(BotPlatform), platform))
                return QuestionErrors.PlatformRequired;

            if (!Enum.IsDefined(typeof(QuestionType), type))
                return QuestionErrors.TypeRequired;

            if (customer is null)
                return QuestionErrors.CustomerNameRequired;

            if (string.IsNullOrWhiteSpace(customer.Name))
                return QuestionErrors.CustomerNameRequired;

            if (string.IsNullOrWhiteSpace(message))
                return QuestionErrors.EmptyMessage;

            if (message.Length > 1000)
                return QuestionErrors.MessageTooLong;

            return Result.Success;
        }

        #endregion
    }
}
