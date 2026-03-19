using Botify.Domain.Models.Bots; // تأكد من الـ namespace الصحيح لـ BotDailyStats
using Botify.Domain.Models.Bots.ValueObjects;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Shops;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedShopAndBotStatsExtensions
{
    public static async Task SeedShopAndStatsAsync(this ApplicationDbContext context, Guid merchantId, Guid botId)
    {
        // 1. إضافة إعدادات المتجر (Shop Settings)
        if (!await context.ShopSettings.AnyAsync(s => s.MerchantId == merchantId))
        {
            // نفترض أن ShopSettings.Create تأخذ (Id, MerchantId, StoreName, Currency, Logo, etc.)
            var shopSettings = ShopSettings.CreateDefault(merchantId);

            context.ShopSettings.Add(shopSettings);
        }

        

        await context.SaveChangesAsync();
    }
}