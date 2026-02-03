using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.SupportRequests
{
    public sealed class SupportRequest : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public Guid BotId { get; private set; }
        public Guid CustomerId { get; private set; }
        public string BotName { get; private set; } = string.Empty;
        public BotPlatform Platform { get; private set; }
        public SupportRequestType Type { get; private set; }
        public RequestReason Reason { get; private set; }

        public Customer Customer { get;}   
        public string Message { get; private set; } = string.Empty;
        public bool IsResolved { get; private set; } = false;
        public DateTime? ResolvedAt { get; private set; }

        private SupportRequest() { }

        private SupportRequest(
            Guid id,
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            SupportRequestType type,
            RequestReason reason,
            Guid customerId,
            string message)
            : base(id)
        {
            MerchantId = merchantId;
            BotId = botId;
            BotName = botName;
            Platform = platform;
            Type = type;
            Reason = reason;
            CustomerId = customerId;
            Message = message;
            IsResolved = false;
            ResolvedAt = null;
        }


        #region Factory

        public static Result<SupportRequest> Create(
            Guid id,
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            SupportRequestType type,
            RequestReason reason,
            Guid customerId,
            string message)

        {
            var validationResult = ValidateInputs(id, merchantId, botId, botName, platform, type, reason, customerId, message);
            if (!validationResult.IsSuccess)
                return validationResult.Errors;

            var supportRequest = new SupportRequest(
                id,
                merchantId,
                botId,
                botName,
                platform,
                type,
                reason,
                customerId,
                message);

            return supportRequest;

        }

        #endregion

        #region Domain Operations

        public Result<Success> Resolve()
        {
            if (IsResolved)
                return SupportRequestErrors.AlreadyResolved;

            IsResolved = true;
            ResolvedAt = DateTime.UtcNow;
            return Result.Success;
        }

        public Result<Success> Unresolve()
        {
            if (!IsResolved)
                return SupportRequestErrors.NotResolved;

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
            SupportRequestType type,
            RequestReason reason,
            Guid customerId,
            string message)
        {
            if (id == Guid.Empty)
                return SupportRequestErrors.IdRequired;

            if (merchantId == Guid.Empty)
                return SupportRequestErrors.MerchantIdRequired;

            if (botId == Guid.Empty)
                return SupportRequestErrors.BotIdRequired;

            if (string.IsNullOrWhiteSpace(botName))
                return SupportRequestErrors.BotNameRequired;

            if (!Enum.IsDefined(typeof(BotPlatform), platform))
                return SupportRequestErrors.PlatformRequired;

            if (!Enum.IsDefined(typeof(SupportRequestType), type))
                return SupportRequestErrors.TypeRequired;

            if (!Enum.IsDefined(typeof(RequestReason), reason))
                return SupportRequestErrors.ReasonRequired;

            if (customerId == Guid.Empty)
                return SupportRequestErrors.CustomerNameRequired;

            if (string.IsNullOrWhiteSpace(message))
                return SupportRequestErrors.EmptyMessage;

            if (message.Length > 1000)
                return SupportRequestErrors.MessageTooLong;

            return Result.Success;
        }


        #endregion
    }
}
