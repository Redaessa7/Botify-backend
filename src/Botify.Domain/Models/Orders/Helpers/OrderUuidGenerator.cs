using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Domain.Models.Orders.Helpers;
public static class OrderUuidGenerator
{
    public static string Generate(Guid merchantId, Guid customerId)
    {
        var merchantPart = merchantId.ToString().Substring(0, 8); // short merchant code
        var customerPart = customerId.ToString().Substring(0, 8); // short customer code
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomPart = Guid.NewGuid().ToString().Substring(0, 4); // add some randomness
        return $"ORD-{merchantPart}-{customerPart}-{timestamp}-{randomPart}";
    }
}
