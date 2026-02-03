namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedIds
{
    // Products
    public static readonly Guid ProductHeadphones = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid ProductSmartWatch = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ProductCoffeeMaker = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    // Customers
    public static readonly Guid CustomerJohnDoe = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public static readonly Guid CustomerFatimaAli = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid CustomerCarlos = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    // Orders
    public static readonly Guid OrderPending = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OrderConfirmed = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OrderDelivered = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OrderCancelled = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid BotDefaultTelegram = Guid.Parse("55555555-5555-5555-5555-555555555555");
}
