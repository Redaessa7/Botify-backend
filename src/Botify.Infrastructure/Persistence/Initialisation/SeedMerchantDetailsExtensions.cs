using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedMerchantDetailsExtensions
{
    public static async Task SeedMerchantWithBotAndQuestionsAsync(
        this ApplicationDbContext context,
        Guid merchantId,
        string email,
        string password,
        string phone,
        string firstName,
        string lastName,
        string bio,
        string profileImageUrl,
        string language,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new ApplicationUser { Email = email, UserName = email, PhoneNumber = phone, EmailConfirmed = true };
        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            logger.LogWarning($"Failed to create identity user '{email}'");
            return;
        }

        await userManager.AddToRoleAsync(user, Roles.MERCHANT.ToString());

        var profileResult = UserProfile.Create(Guid.NewGuid(), user.Id, firstName, lastName, email, bio, profileImageUrl, language);
        if (profileResult.IsError) return;

        var merchantResult = Merchant.Create(merchantId, profileResult.Value);
        if (merchantResult.IsError) return;

        var merchant = merchantResult.Value;

        var botResult = merchant.AddBot(SeedIds.BotDefaultTelegram, name: "Default Telegram Bot", "seed-token", BotPlatform.TELEGRAM, "seed-secret", "https://webhook.botify.com");
        if (!botResult.IsSuccess) return;

        var bot = botResult.Value;

        merchant.Approve();

        context.UserProfiles.Add(profileResult.Value);
        context.Merchants.Add(merchant);
        context.Bots.Add(bot);

        await context.SeedLegacyProductsAsync(merchant.Id);
        await context.SeedCustomersAsync(merchant.Id);
        await context.SeedOrdersAsync(merchant.Id);
        await context.SaveChangesAsync();

        logger.LogInformation($"Seeded merchant '{email}' with bot and questions.");
    }


}
