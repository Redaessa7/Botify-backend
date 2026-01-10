using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Domain.Models.Orders.Enums
{
    public enum OrderStatus { Pending = 0, Confirmed = 1, Preparing = 2, Delivered = 3, Cancelled = 4 }
}
