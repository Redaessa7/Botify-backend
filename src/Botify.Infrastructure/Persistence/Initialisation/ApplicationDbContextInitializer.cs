using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation
{
    public class ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager
    )
    {
        private readonly ILogger<ApplicationDbContextInitializer> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task InitialiseAsync()
        {
            try
            {
                // Ensure SQLite directory exists
                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                var builder = new SqliteConnectionStringBuilder(connectionString);

                var dataSource = builder.DataSource;
                var directory = Path.GetDirectoryName(dataSource);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Apply migrations if needed
                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            // --- Roles ---
            var roles = new[]
            {
                Roles.MERCHANT.ToString(),
                Roles.ADMIN.ToString(),
                Roles.USER.ToString()
            };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole { Name = roleName });

                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogWarning($"Failed to create role '{roleName}': {errors}");
                    }
                }
            }

            // --- Merchants Seed Data ---
            if (!_context.Merchants.Any())
            {
                var merchantsData = new[]
                {
                    new
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Email = "merchant1@botify.com",
                        Password = "Merchant123@",
                        Phone = "500000001",
                        FirstName = "Ali",
                        LastName = "Saleh",
                        Bio = "Seeded merchant for Botify",
                        ProfileImageUrl = "https://example.com/images/merchant1.png",
                        Language = "en"
                    },
                    new
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Email = "merchant2@botify.com",
                        Password = "Merchant123@",
                        Phone = "500000002",
                        FirstName = "Fatima",
                        LastName = "Hassan",
                        Bio = "Second seeded merchant",
                        ProfileImageUrl = "https://example.com/images/merchant2.png",
                        Language = "ar"
                    }
                };

                foreach (var data in merchantsData)
                {
                    await SeedMerchantWithBotAndQuestionAsync(data.Id, data.Email, data.Password, data.Phone,
                        data.FirstName, data.LastName, data.Bio, data.ProfileImageUrl, data.Language);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedMerchantWithBotAndQuestionAsync(
            Guid merchantId,
            string email,
            string password,
            string phone,
            string firstName,
            string lastName,
            string bio,
            string profileImageUrl,
            string language)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
                return;

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                PhoneNumber = phone,
                EmailConfirmed = true
            };

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                _logger.LogWarning($"Failed to create identity user '{email}'");
                return;
            }

            await _userManager.AddToRoleAsync(user, Roles.MERCHANT.ToString());

            // Profile
            var profileResult = UserProfile.Create(
                Guid.NewGuid(),
                user.Id,
                firstName,
                lastName,
                email,
                bio,
                profileImageUrl,
                language
            );
            if (profileResult.IsError) return;

            // Merchant
            var merchantResult = Merchant.Create(merchantId, profileResult.Value);
            if (merchantResult.IsError) return;

            var merchant = merchantResult.Value;

            // Add Bot via Merchant.AddBot
            var botId = Guid.NewGuid();
            var botResult = merchant.AddBot(
                botId,
                "Default Telegram Bot",
                "seed-token",
                "TELEGRAM",
                "seed-secret",
                "https://webhook.botify.com"
            );
            if (!botResult.IsSuccess) return;

            var bot = botResult.Value;

            // Question
            var customerInfoResult = CustomerInfo.Create(
                "Test Customer",
                "customer@example.com",
                "telegramUser",
                "whatsappUser",
                "messengerUser"
            );
            if (customerInfoResult.IsError) return;

            var questionResult = Question.Create(
                Guid.NewGuid(),
                merchantId,
                bot.Id,
                bot.Name,
                bot.Platform,
                QuestionType.GENERAL,
                customerInfoResult.Value,
                "Hello, I need help with my order."
            );
            if (questionResult.IsError) return;

            var question = questionResult.Value;

            // Attach to merchant
            merchant.Approve(); // optional: mark merchant approved
            _context.UserProfiles.Add(profileResult.Value);
            _context.Merchants.Add(merchant);
            _context.Bots.Add(bot);
            _context.Questions.Add(question);

            _logger.LogInformation($"Seeded merchant '{email}' with bot and question.");
        }
        }
    }
