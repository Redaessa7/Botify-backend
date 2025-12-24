using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class ContactInfoResponse
    {
        public string CustomerName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Telegram { get; init; }
        public string? Whatsapp { get; init; }
        public string? Messenger { get; init; }

        public static ContactInfoResponse FromDomain(CustomerInfo customer) =>
            new()
            {
                CustomerName = customer.Name,
                Email = customer.Email,
                Telegram = customer.Telegram,
                Whatsapp = customer.Whatsapp,
                Messenger = customer.Messenger
            };
    }
}
