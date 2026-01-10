using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.ValueObjects;
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

        var botResult = merchant.AddBot(Guid.NewGuid(), "Default Telegram Bot", "seed-token", BotPlatform.TELEGRAM, "seed-secret", "https://webhook.botify.com");
        if (!botResult.IsSuccess) return;

        var bot = botResult.Value;
        var questions = SeedQuestions(merchantId, bot).ToList();
        foreach (var q in questions) merchant.AddQuestion(q);

        merchant.Approve();

        context.UserProfiles.Add(profileResult.Value);
        context.Merchants.Add(merchant);
        context.Bots.Add(bot);
        context.Questions.AddRange(questions);

        await context.SeedLegacyProductsAsync(merchant.Id);
        await context.SeedCustomersAsync(merchant.Id);
        await context.SeedOrdersAsync(merchant.Id);
        await context.SaveChangesAsync();

        logger.LogInformation($"Seeded merchant '{email}' with bot and questions.");
    }

    private static IEnumerable<Question> SeedQuestions(Guid merchantId, Bot bot)
    {
        var customers = new[]
        {
            new { Name = "John Smith", Email = "john.smith@example.com", Telegram = "john_smith_tg", Whatsapp = "johnsmith_wa", Messenger = "johnsmith_fb" },
            new { Name = "Fatima Ali", Email = "fatima.ali@example.com", Telegram = "fatima_ali_tg", Whatsapp = "fatimaali_wa", Messenger = "fatimaali_fb" }
        };

        var messages = new[]
        {
            "Hello, I need help with my order.",
            "Can you tell me the delivery time?",
            "Do you have discounts available?"
        };

        int i = 0;
        foreach (var message in messages)
        {
            var customer = customers[i % customers.Length];
            var customerInfoResult = CustomerInfo.Create(customer.Name, customer.Email, customer.Telegram, customer.Whatsapp, customer.Messenger);
            if (customerInfoResult.IsError) continue;

            var questionResult = Question.Create(Guid.NewGuid(), merchantId, bot.Id, bot.Name, bot.Platform, QuestionType.GENERAL, customerInfoResult.Value, message);
            if (questionResult.IsSuccess) yield return questionResult.Value;

            i++;
        }
    }
}
