using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Domain.Events.Merchants
{
    public record MerchantApproved(Guid MerchantId) : DomainEventBase;

    public record BotActivated(Guid MerchantId, Guid BotId, string Platform) : DomainEventBase;

    public record BotDeactivated(Guid MerchantId, Guid BotId) : DomainEventBase;
}
