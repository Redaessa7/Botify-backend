using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Identity.Enums;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Shops;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.Merchants.Services;

public sealed class MerchantService : IMerchantService
{
    private readonly IApplicationDbContext _db;
    private readonly IUser _user;
    private readonly IBotSecretGenerator _secretGenerator;
    private readonly IServiceProvider _serviceProvider;
    private readonly WebhookOptions _webhookOptions;
    private readonly ILogger<MerchantService> _logger;

    public MerchantService(
        IApplicationDbContext db,
        IUser user,
        IBotSecretGenerator secretGenerator,
        IServiceProvider serviceProvider,
        IOptions<WebhookOptions> webhookOptions,
        ILogger<MerchantService> logger)
    {
        _db = db;
        _user = user;
        _secretGenerator = secretGenerator;
        _serviceProvider = serviceProvider;
        _webhookOptions = webhookOptions.Value;
        _logger = logger;
    }

    #region Merchant Lifecycle

    public async Task<Result<HomePageInitDataResponse>> GetHomePageAsync(Guid merchantId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching homepage data for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants
            .Include(m => m.Profile)
            .Include(m => m.Subscription)
            .Include(m => m.Products)
            .Include(m => m.Orders)
            .Include(m => m.AIConfiguration)
            .Include(m => m.Customers)
            .Include(m => m.ShopSettings)
            .Include(m => m.Bots).ThenInclude(b => b.DailyStats)
            .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        _logger.LogInformation("Homepage data retrieved successfully for MerchantId={MerchantId}", merchantId);
        return merchant.ToHomePageInitDataResponse();
    }

    public async Task<Result<Merchant>> CreateAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting merchant creation for IdentityUserId={IdentityUserId}", request.IdentityUserId);

        var profileResult = UserProfile.Create(
            Guid.NewGuid(),
            request.IdentityUserId,
            request.Profile.FirstName,
            request.Profile.LastName,
            request.Profile.Email,
            request.Profile.Bio,
            request.Profile.ProfileImageUrl,
            request.Profile.Language
        );

        if (!profileResult.IsSuccess)
        {
            _logger.LogWarning("Failed to create user profile for IdentityUserId={IdentityUserId}. Errors={Errors}",
                request.IdentityUserId, profileResult.Errors);
            return profileResult.Errors;
        }

        var merchantResult = Merchant.Create(Guid.NewGuid(), profileResult.Value);
        if (!merchantResult.IsSuccess)
        {
            _logger.LogWarning("Failed to create merchant for IdentityUserId={IdentityUserId}. Errors={Errors}",
                request.IdentityUserId, merchantResult.Errors);
            return merchantResult.Errors;
        }

        _db.UserProfiles.Add(profileResult.Value);
        _db.Merchants.Add(merchantResult.Value);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Merchant created successfully with MerchantId={MerchantId}", merchantResult.Value.Id);
        return merchantResult;
    }

    public async Task<Result<Success>> UpdatePreferencesAsync(Guid merchantId, QuickActions preferences, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating preferences for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.UpdatePreferences(preferences);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update preferences for MerchantId={MerchantId}. Errors={Errors}", merchantId, result.Errors);
            return result.Errors;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Preferences updated successfully for MerchantId={MerchantId}", merchantId);

        return Result.Success;
    }

    public async Task<Result<Success>> DeleteAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting merchant MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants
                            .Include(x => x.Profile)
                            .FirstOrDefaultAsync(x => x.Id == merchantId, cancellationToken);

        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }
        var user = _user.UserInfo();

        if (user.Roles.Contains(Roles.ADMIN.ToString()) && user.Id == merchant.Profile.IdentityUserId)
        {
            _logger.LogWarning("Delete failed: Admin cannot delete their own account. MerchantId={MerchantId}", merchantId);
            return MerchantErrors.AdminCannotDeleteSelf;
        }


        merchant.SoftDelete();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Merchant deleted successfully MerchantId={MerchantId}", merchantId);
        return Result.Success;
    }

    public async Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Approving merchant MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.FirstOrDefaultAsync(x => x.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.Approve();
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to approve MerchantId={MerchantId}. Errors={Errors}", merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Merchant approved successfully MerchantId={MerchantId}", merchantId);

        return result;
    }

    public async Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Suspending merchant MerchantId={MerchantId} Reason={Reason}", merchantId, reason);

        var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.Suspend(reason);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to suspend MerchantId={MerchantId}. Errors={Errors}", merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Merchant suspended successfully MerchantId={MerchantId}", merchantId);

        return result;
    }

    #endregion

    #region Shop Management

    public async Task<Result<ShopResponse>> SetShopAsync(Guid merchantId, SetShopRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting shop for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.Include(x => x.ShopSettings).FirstOrDefaultAsync(x => x.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        ShopSettings? shopSettings = null;
        if (merchant.ShopSettings is null)
        {
            var defaultShopResult = ShopSettings.Create(
                merchantId,
                request.ShopName,
                request.Currency,
                request.Language,
                request.Address,
                request.IndustryName,
                request.BusinessEmail,
                request.BusinessPhoneNumber,
                request.SocialMediaLinks,
                request.LogoImageUrl,
                request.CoverImageUrl,
                request.TaxRate,
                request.Description,
                request.Timezone,
                request.DeliveryEnabled,
                request.PickupEnabled,
                request.MinimumOrder,
                request.DeliveryFee,
                request.IsOpen,
                request?.OpeningHours?.ToDomain()
            );

            if (!defaultShopResult.IsSuccess)
            {
                _logger.LogWarning("Failed to create shop settings for MerchantId={MerchantId}. Errors={Errors}", merchantId, defaultShopResult.Errors);
                return defaultShopResult.Errors;
            }

            shopSettings = defaultShopResult.Value;
            var assignResult = merchant.AssignShop(shopSettings);
            if (!assignResult.IsSuccess)
            {
                _logger.LogWarning("Failed to assign shop settings for MerchantId={MerchantId}. Errors={Errors}", merchantId, assignResult.Errors);
                return assignResult.Errors;
            }

            _db.ShopSettings.Add(shopSettings);
        }
        else
        {
            var settingsResult = merchant.ShopSettings.Update(
                request.ShopName,
                request.Currency,
                request.Language,
                request.Address,
                request.IndustryName,
                request.BusinessEmail,
                request.BusinessPhoneNumber,
                request.SocialMediaLinks,
                request.LogoImageUrl,
                request.CoverImageUrl,
                request.TaxRate,
                request.Description,
                request.Timezone,
                request.DeliveryEnabled,
                request.PickupEnabled,
                request.MinimumOrder,
                request.DeliveryFee,
                request.IsOpen,
                request?.OpeningHours?.ToDomain()
            );

            if (!settingsResult.IsSuccess)
            {
                _logger.LogWarning("Failed to update shop settings for MerchantId={MerchantId}. Errors={Errors}", merchantId, settingsResult.Errors);
                return settingsResult.Errors;
            }

            shopSettings = merchant.ShopSettings!;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Shop settings saved successfully for MerchantId={MerchantId}", merchantId);

        return ShopResponse.FromDomain(shopSettings);
    }

    public async Task<Result<bool>> CheckShopExistsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if shop exists for MerchantId={MerchantId}", merchantId);

        var exists = await _db.Merchants.AnyAsync(m => m.Id == merchantId && m.ShopSettings != null, cancellationToken);

        _logger.LogInformation("Shop existence check for MerchantId={MerchantId}: Exists={Exists}", merchantId, exists);
        return exists;
    }

    public async Task<Result<Success>> DeleteShopAsync(Guid merchantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting shop for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.Include(m => m.ShopSettings).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        if (merchant.ShopSettings is null)
        {
            _logger.LogWarning("Shop not found for MerchantId={MerchantId}", merchantId);
            return Error.NotFound("Merchant.Shop.NotFound", "Shop not found for this merchant.");
        }

        _db.ShopSettings.Remove(merchant.ShopSettings);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Shop deleted successfully for MerchantId={MerchantId}", merchantId);
        return Result.Success;
    }

    #endregion

    #region Bot Management

    public async Task<Result<Success>> DeleteBotAsync(Guid merchantId, Guid botId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting bot BotId={BotId} for MerchantId={MerchantId}", botId, merchantId);

        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var bot = merchant.Bots.FirstOrDefault(b => b.Id == botId);
        if (bot is null)
        {
            _logger.LogWarning("Bot not found BotId={BotId} for MerchantId={MerchantId}", botId, merchantId);
            return Error.NotFound("Merchant.Bot.NotFound", "The specified bot could not be found.");
        }

        var platformService = _serviceProvider.GetRequiredKeyedService<IBotPlatformService>(bot.Platform.AsKey());
        if (platformService is null)
        {
            _logger.LogError("Invalid bot platform for BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
            return BotErrors.PlatformInvalid;
        }

        var webhookResult = await platformService.RemoveWebhookAsync(bot.Token, ct);
        if (!webhookResult.IsSuccess)
        {
            _logger.LogError("Failed to remove webhook for BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
            return BotErrors.WebhookRemovalFailed;
        }

        bot.SoftDelete();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Bot deleted successfully BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
        return Result.Success;
    }

    public async Task<Result<BotResponse>> CreateBotAsync(Guid merchantId, CreateBotRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating bot for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var platformService = _serviceProvider.GetRequiredKeyedService<IBotPlatformService>(request.Platform.AsKey());
        if (platformService is null)
        {
            _logger.LogError("Invalid bot platform for MerchantId={MerchantId}", merchantId);
            return BotErrors.PlatformInvalid;
        }

        var tokenCheck = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        if (!tokenCheck.IsSuccess)
        {
            _logger.LogWarning("Invalid bot token for MerchantId={MerchantId}", merchantId);
            return tokenCheck.Errors;
        }

        var secret = _secretGenerator.Generate();
        var botResult = merchant.AddBot(Guid.NewGuid(), request.Name, request.ApiToken, request.Platform, secret, _webhookOptions.BaseUrl);
        if (!botResult.IsSuccess)
        {
            _logger.LogWarning("Failed to add bot for MerchantId={MerchantId}. Errors={Errors}", merchantId, botResult.Errors);
            return botResult.Errors;
        }

        var bot = botResult.Value;
        _db.Bots.Add(bot);
        await _db.SaveChangesAsync(ct);

        var webhookResult = await platformService.SetWebhookAsync(bot.Token, bot.WebhookUrl!, bot.Secret, ct);
        if (!webhookResult.IsSuccess)
        {
            _logger.LogError("Failed to register webhook for BotId={BotId} MerchantId={MerchantId}", bot.Id, merchantId);
            return BotErrors.WebhookRegistrationFailed;
        }

        _logger.LogInformation("Bot created successfully BotId={BotId} MerchantId={MerchantId}", bot.Id, merchantId);
        return BotResponse.FromDomain(bot);
    }

    #endregion

    #region Queries

    public async Task<Result<CurrentMerchantResponse>> GetCurrentMerchantAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching current merchant for UserId={UserId}", _user.Id);

        var merchant = await _db.Merchants
            .Include(m => m.ShopSettings)
            .Include(m => m.AIConfiguration)
            .Include(m => m.Profile)
            .Include(m => m.Subscription)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null)
        {
            _logger.LogWarning("Current merchant not found for UserId={UserId}", _user.Id);
            return MerchantErrors.NotFound;
        }

        _logger.LogInformation("Current merchant retrieved successfully MerchantId={MerchantId}", merchant.Id);
        return merchant.ToCurrentResponse();
    }

    public async Task<Result<Updated>> UpdateBasicInfoAsync(Guid merchantId, UpdateMerchantBasicInfoRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating basic info for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.Include(m => m.Profile).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.UpdateBasicInfo(request.FirstName, request.LastName, request.Bio, request.ProfileImageUrl);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update basic info for MerchantId={MerchantId}. Errors={Errors}", merchantId, result.Errors);
            return result.Errors;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Basic info updated successfully for MerchantId={MerchantId}", merchantId);

        return Result.Updated;
    }

    public async Task<Result<ShopResponse>> GetShopAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching shop for MerchantId={MerchantId}", merchantId);

        var shop = await _db.Merchants
            .Include(m => m.ShopSettings).ThenInclude(s => s.SocialMediaLinks)
            .Include(m => m.ShopSettings.OpeningHours)
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (shop is null || shop.ShopSettings is null)
        {
            _logger.LogWarning("Shop not found for MerchantId={MerchantId}", merchantId);
            return ShopErrors.NotFound;
        }

        _logger.LogInformation("Shop retrieved successfully for MerchantId={MerchantId}", merchantId);
        return ShopResponse.FromDomain(shop.ShopSettings);
    }

    public async Task<Result<BotDetailsResponse>> GetBotByIdAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching bot BotId={BotId} for MerchantId={MerchantId}", botId, merchantId);

        var bot = await _db.Bots
            .Include(b => b.DailyStats)
            .Include(b => b.Questions)
            .Where(b => b.MerchantId == merchantId && b.Id == botId)
            .FirstOrDefaultAsync(ct);

        if (bot is null)
        {
            _logger.LogWarning("Bot not found BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
            return BotErrors.BotNotFound;
        }

        _logger.LogInformation("Bot retrieved successfully BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
        return bot.ToBotDetailsResponse();
    }

    public async Task<Result<List<GetListBotResponse>>> GetBotsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching bots for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var bots = merchant.Bots.Select(GetListBotResponse.FromDomain).ToList();
        _logger.LogInformation("Bots retrieved successfully for MerchantId={MerchantId}. Count={Count}", merchantId, bots.Count);

        return bots;
    }

    public async Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating bot BotId={BotId} for MerchantId={MerchantId}", botId, merchantId);

        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.ActivateBot(botId);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to activate bot BotId={BotId} MerchantId={MerchantId}. Errors={Errors}", botId, merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bot activated successfully BotId={BotId} MerchantId={MerchantId}", botId, merchantId);

        return result;
    }

    public async Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating bot BotId={BotId} for MerchantId={MerchantId}", botId, merchantId);

        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var result = merchant.DeactivateBot(botId);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate bot BotId={BotId} MerchantId={MerchantId}. Errors={Errors}", botId, merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bot deactivated successfully BotId={BotId} MerchantId={MerchantId}", botId, merchantId);

        return result;
    }
public async Task<Result<Success>> UpdateBotNameAsync(Guid merchantId, Guid botId, string newName, CancellationToken ct = default)
{
    _logger.LogInformation("Updating bot name for BotId={BotId} MerchantId={MerchantId}", botId, merchantId);

    var merchant = await _db.Merchants
        .Include(m => m.Bots)
        .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

    if (merchant is null)
    {
        _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
        return MerchantErrors.NotFound;
    }

    var bot = merchant.Bots.FirstOrDefault(b => b.Id == botId);
    if (bot is null)
    {
        _logger.LogWarning("Bot not found BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
        return BotErrors.BotNotFound;
    }

    if (string.IsNullOrWhiteSpace(newName))
    {
        _logger.LogWarning("Bot name update failed: name required for BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
        return BotErrors.NameRequired;
    }

    if (newName.Length > 100)
    {
        _logger.LogWarning("Bot name update failed: name too long for BotId={BotId} MerchantId={MerchantId}", botId, merchantId);
        return BotErrors.NameTooLong;
    }

    bot.UpdateName(newName);
    await _db.SaveChangesAsync(ct);

    _logger.LogInformation("Bot name updated successfully BotId={BotId} MerchantId={MerchantId} NewName={NewName}", botId, merchantId, newName);

    return Result.Success;
}

  #endregion
}
