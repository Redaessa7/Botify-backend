using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedMerchantsExtensions
{
    public static async Task SeedMerchantsAsync(this ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var existingMerchant = await context.Merchants
                                .IgnoreQueryFilters() 
                                .AnyAsync();
        if (existingMerchant) return;

        var merchantsData = new[]
        {
            new { Id  = Guid.Parse("11111111-1111-1111-1111-111111111111"), Email = "xmohammedfares@gmail.com", Password = "Merchant123@", Phone = "500000001", FirstName = "Ali", LastName = "Saleh", Bio = "Seeded merchant for Botify", ProfileImageUrl = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcdp5nS2uQjEmrXugF3JMnfKaviP1LZc5z8Sk2", Language = "en" },
        };


        foreach (var data in merchantsData)
        {
            await context.SeedMerchantWithBotAndQuestionsAsync(
                data.Id, data.Email, data.Password, data.Phone,
                data.FirstName, data.LastName, data.Bio, data.ProfileImageUrl, data.Language,
                userManager, logger);
        }

        await context.SaveChangesAsync();
    }
}
