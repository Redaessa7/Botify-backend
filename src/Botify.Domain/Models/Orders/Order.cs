using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Orders
{
    public sealed class Order : AuditableEntity
    {
        public string Uuid { get; private set; } = string.Empty; // <-- string UUID

        public Guid MerchantId { get; private set; }
        public Guid CustomerId { get; private set; }
        public string CustomerName { get; private set; } = string.Empty;
        public string CustomerContact { get; private set; } = string.Empty;
        public BotPlatform Platform { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();
        public decimal TotalAmount { get; private set; }
        public string Currency { get; private set; } = string.Empty;
        public OrderStatus Status { get; private set; }
        public string? DeliveryAddress { get; private set; }
        public string? Notes { get; private set; }
        public Customer? Customer { get; }
        private Order() { }

        private Order(
            Guid id,
            string uuid,                  // <-- accept UUID string
            Guid merchantId,
            Guid customerId,
            string customerName,
            string customerContact,
            BotPlatform platform,
            List<OrderItem> items,
            decimal totalAmount,
            string currency,
            OrderStatus status,
            string? deliveryAddress,
            string? notes) : base(id)
        {
            Uuid = uuid;                  // <-- assign from outside
            MerchantId = merchantId;
            CustomerId = customerId;
            CustomerName = customerName;
            CustomerContact = customerContact;
            Platform = platform;
            Items = items;
            TotalAmount = totalAmount;
            Currency = currency;
            Status = status;
            DeliveryAddress = deliveryAddress;
            Notes = notes;
        }

        public static Result<Order> Create(
            Guid id,
            string uuid,                  
            Guid merchantId,
            Guid customerId,
            string customerName,
            string customerContact,
            BotPlatform platform,
            List<OrderItem> items,
            decimal totalAmount,
            string currency,
            OrderStatus status,
            string? deliveryAddress = null,
            string? notes = null)
        {
            var validation = ValidateInputs(id, uuid, merchantId, customerId, customerName, customerContact, platform, items, totalAmount, currency, status);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new Order(id, uuid, merchantId, customerId, customerName, customerContact, platform, items, totalAmount, currency, status, deliveryAddress, notes);
        }

        public Result<Updated> UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
            return Result.Updated;
        }

        #region Validation

        private static Result<Success> ValidateInputs(
            Guid id,
            string uuid,                  // <-- validate UUID string
            Guid merchantId,
            Guid customerId,
            string customerName,
            string customerContact,
            BotPlatform platform,
            List<OrderItem> items,
            decimal totalAmount,
            string currency,
            OrderStatus status)
        {
            if (id == Guid.Empty)
                return OrderErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(uuid))
                return OrderErrors.UuidRequired; // <-- add a new error type

            if (merchantId == Guid.Empty)
                return OrderErrors.MerchantIdRequired;

            if (customerId == Guid.Empty)
                return OrderErrors.CustomerIdRequired;

            if (string.IsNullOrWhiteSpace(customerName))
                return OrderErrors.CustomerNameRequired;

            if (string.IsNullOrWhiteSpace(customerContact))
                return OrderErrors.CustomerContactRequired;

            if (!Enum.IsDefined(typeof(BotPlatform), platform))
                return OrderErrors.PlatformInvalid;

            if (items == null || items.Count == 0)
                return OrderErrors.ItemsRequired;

            if (totalAmount <= 0)
                return OrderErrors.TotalAmountInvalid;

            if (string.IsNullOrWhiteSpace(currency))
                return OrderErrors.CurrencyRequired;

            if (!Enum.IsDefined(typeof(OrderStatus), status))
                return OrderErrors.StatusInvalid;

            return Result.Success;
        }

        #endregion
    }
}
