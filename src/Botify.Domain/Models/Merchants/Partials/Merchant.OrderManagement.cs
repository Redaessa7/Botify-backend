using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        private readonly List<Order> _orders = new();
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    }
}
