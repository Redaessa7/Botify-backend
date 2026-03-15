namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedIds
{
    // --- Products (Electronics Tech Store) ---
    public static readonly Guid ProductHeadphones = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid ProductSmartWatch = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid ProductCoffeeMaker = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid ProductLaptop = Guid.Parse("f1111111-1111-1111-1111-111111111111");
    public static readonly Guid ProductMonitor = Guid.Parse("f2222222-2222-2222-2222-222222222222");
    public static readonly Guid ProductKeyboard = Guid.Parse("f3333333-3333-3333-3333-333333333333");
    public static readonly Guid ProductDeskChair = Guid.Parse("f4444444-4444-4444-4444-444444444444");
    public static readonly Guid ProductAirFryer = Guid.Parse("f5555555-5555-5555-5555-555555555555");
    public static readonly Guid ProductWaterBottle = Guid.Parse("f6666666-6666-6666-6666-666666666666");
    public static readonly Guid ProductYogaMat = Guid.Parse("f7777777-7777-7777-7777-777777777777");

    // --- Customers ---
    public static readonly Guid CustomerJohnDoe = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public static readonly Guid CustomerFatimaAli = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid CustomerCarlos = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    // --- Orders ---
    public static readonly Guid OrderPending = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OrderConfirmed = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OrderDelivered = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OrderCancelled = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // --- Bot & System ---
    public static readonly Guid BotDefaultTelegram = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid MerchantPrimary = Guid.Parse("11111111-1111-1111-1111-111111111111");
    // --- Shop & Bot Settings ---
    public static readonly Guid ShopSettingsId = Guid.Parse("77777777-7777-7777-7777-777777777777");
}