using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Domain.Models.Customers.ValueObjects
{

    public sealed class MessengerProfile
    {
        public string? Link { get; init; }
        public string? ChatId { get; init; }
    }
}
