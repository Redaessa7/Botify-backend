using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class ContactInfoResponse
    {
        public string CustomerName { get; init; } = string.Empty;
        public string? PlatformContact { get; init; }
     
    }
}
