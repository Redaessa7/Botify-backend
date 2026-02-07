using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Shops;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.Merchants.Services;

public sealed class MerchantService : IMerchantService
{
    private readonly IApplicationDbContext _db;
    private readonly IUser _user;
    private readonly IBotSecretGenerator _secretGenerator;
    private readonly IServiceProvider _serviceProvider;
    private readonly WebhookOptions _webhookOptions;

    public MerchantService(
        IApplicationDbContext db,
        IUser user,
        IBotSecretGenerator secretGenerator,
        IServiceProvider serviceProvider,
        IOptions<WebhookOptions> webhookOptions )
    {
        _db = db;
        _user = user;
        _secretGenerator = secretGenerator;
        _serviceProvider = serviceProvider;
        _webhookOptions = webhookOptions.Value;
    }

    #region Merchant Lifecycle

    public async Task<Result<Merchant>> CreateAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
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
            return profileResult.Errors;

        var profile = profileResult.Value;
        _db.UserProfiles.Add(profile);

        var merchantResult = Merchant.Create(
            Guid.NewGuid(),
            profile
        );

        if (!merchantResult.IsSuccess)
            return merchantResult.Errors;

        var merchant = merchantResult.Value;
        

        _db.Merchants.Add(merchantResult.Value);

        await _db.SaveChangesAsync(cancellationToken);

        return merchantResult;
    }

    public async Task<Result<Success>> UpdatePreferencesAsync(Guid merchantId, QuickActions preferences, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, ct); 
        if (merchant is null) 
            return MerchantErrors.NotFound; 
        var result = merchant.UpdatePreferences(preferences);

        if (result is not { IsSuccess: true })
            return result.Errors;

        await _db.SaveChangesAsync(ct);

        return Result.Success;
    }
    
    
    public async Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.FirstOrDefaultAsync(x => x.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var result = merchant.Approve();
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var result = merchant.Suspend(reason);
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion

    #region Shop Management

    public async Task<Result<ShopResponse>> SetShopAsync(Guid merchantId, SetShopRequest request, CancellationToken cancellationToken = default)
    {
        // Load aggregate root
        var merchant = await _db.Merchants.Include(x=> x.ShopSettings).FirstOrDefaultAsync(x=> x.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        ShopSettings? shopSettings = null;
        if (merchant.ShopSettings is null)
        {
            var openHours = request?.OpeningHours?.ToDomain() ?? null;
            // Initialize default shop settings if none exist
            var defaultShopResult = ShopSettings.Create(merchantId, request.ShopName,request.Currency,request.Language,request.Address,request.IndustryName,request.BusinessEmail,request.BusinessPhoneNumber,request.SocialMediaLinks,request.LogoImageUrl,request.CoverImageUrl,request.TaxRate,request.Description,request.Timezone,request.DeliveryEnabled,request.PickupEnabled,request.MinimumOrder,request.DeliveryFee,request.IsOpen, openHours);
           if(!defaultShopResult.IsSuccess)
            {
                return defaultShopResult.Errors;
            }
            shopSettings = defaultShopResult.Value;
            var assignResult = merchant.AssignShop(shopSettings);
            if(!assignResult.IsSuccess)
                return assignResult.Errors;
            _db.ShopSettings.Add(shopSettings);
        }
        else
        {
            // Create ShopSettings via domain factory
            var settingsResult = merchant.ShopSettings.Update(
        name: request.ShopName,
        currency: request.Currency,
        language: request.Language,
        address: request.Address,
        industryName: request.IndustryName,
        businessEmail: request.BusinessEmail,
        businessPhoneNumber: request.BusinessPhoneNumber,
        socialMediaLinks: request.SocialMediaLinks,
        logoImageUrl: request.LogoImageUrl,
        coverImageUrl: request.CoverImageUrl,
        taxRate: request.TaxRate,
        description: request.Description,
        timezone: request.Timezone,
        deliveryEnabled: request.DeliveryEnabled,
        pickupEnabled: request.PickupEnabled,
        minimumOrder: request.MinimumOrder,
        deliveryFee: request.DeliveryFee,
        isOpen: request.IsOpen,
        openingHours: request?.OpeningHours?.ToDomain() ?? null
    );

            if (!settingsResult.IsSuccess) 
                return settingsResult.Errors;
            shopSettings = merchant.ShopSettings!;
        }


        await _db.SaveChangesAsync(cancellationToken);

        return ShopResponse.FromDomain(shopSettings);
    }


    public async Task<Result<bool>> CheckShopExistsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
       var exists = await _db.Merchants
            .AnyAsync(m => m.Id == merchantId && m.ShopSettings != null, cancellationToken);

        return exists;
    }

    #endregion

    #region Bot Management

    public async Task<Result<BotResponse>> CreateBotAsync(Guid merchantId, CreateBotRequest request, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null) return MerchantErrors.NotFound;

        var platformService = _serviceProvider.GetRequiredKeyedService<IBotPlatformService>(request.Platform);

        if(platformService is null)
            return BotErrors.PlatformInvalid;


        var tokenCheck = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        if (!tokenCheck.IsSuccess) return tokenCheck.Errors;

        var secret = _secretGenerator.Generate();

        var botResult = merchant.AddBot(Guid.NewGuid(), request.Name, request.ApiToken, request.Platform, secret, _webhookOptions.BaseUrl);
        if (!botResult.IsSuccess) return botResult.Errors;

        var bot = botResult.Value;
        _db.Bots.Add(bot);
        await _db.SaveChangesAsync(ct);

        var webhookResult = await platformService.SetWebhookAsync(bot.Token, bot.WebhookUrl!, bot.Secret,ct);
        if (!webhookResult.IsSuccess)
        {
            // Optionally mark bot state or log; return error to caller
            return BotErrors.WebhookRegistrationFailed;
        }

        return BotResponse.FromDomain(bot);
    }
    public async Task<Result<BotResponse>> GetBotByIdAsync(Guid merchantId, Guid botId, CancellationToken ct)
    {
       var bot =  await _db.Bots
            .Where(b => b.MerchantId == merchantId && b.Id == botId)
            .FirstOrDefaultAsync(ct);

        if (bot is null) return BotErrors.BotNotFound;

        return BotResponse.FromDomain(bot);
    }
    public async Task<Result<List<GetListBotResponse>>> GetBotsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .Include(m => m.Bots)
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (merchant is null) return MerchantErrors.NotFound;

        var bots = merchant.Bots.Select(GetListBotResponse.FromDomain).ToList();
        return bots;
    }

    public async Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var result = merchant.ActivateBot(botId);
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var result = merchant.DeactivateBot(botId);
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion

    #region Queries

    public async Task<Result<CurrentMerchantResponse>> GetCurrentMerchantAsync(CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .Include(m => m.ShopSettings)
            .Include(m => m.AIConfiguration)
            .Include(m => m.Profile)
            .Include(m => m.Subscription)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null) return MerchantErrors.NotFound;

        return merchant.ToCurrentResponse();
    }
    public async Task<Result<ShopResponse>> GetShopAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var shop = await _db.Merchants
            .Include(m => m.ShopSettings)
            .ThenInclude(s => s.SocialMediaLinks)
            .Include(m => m.ShopSettings.OpeningHours)
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (shop is null || shop.ShopSettings is null)
            return ShopErrors.NotFound;

        return ShopResponse.FromDomain(shop.ShopSettings);
    }
    #endregion
}
