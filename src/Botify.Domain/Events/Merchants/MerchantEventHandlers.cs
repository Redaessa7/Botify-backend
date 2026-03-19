namespace Botify.Domain.Events.Merchants
{
    public class MerchantEventHandlers
    {
        public static void Handle(IDomainEvent domainEvent)
        {
            switch (domainEvent)
            {
                case MerchantApproved e:
                    Console.WriteLine($"✅ Merchant {e.MerchantId} approved at {e.OccurredOn}");
                    // TODO: Add logic here (send email, publish to bus, etc.)
                    break;

                case BotActivated e:
                    Console.WriteLine($"🤖 Bot {e.BotId} activated for Merchant {e.MerchantId} on {e.Platform}");
                    // TODO: Add logic here (notify system, update read model, etc.)
                    break;

                case BotDeactivated e:
                    Console.WriteLine($"🛑 Bot {e.BotId} deactivated for Merchant {e.MerchantId}");
                    break;
            }
        }
    }

}
