using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Responses;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Botify.Application.Services.Merchants;

public sealed class MerchantService : IMerchantService
{
    private readonly IApplicationDbContext _db;
    private readonly IUser _user;
    private readonly IBotSecretGenerator _secretGenerator;
    private readonly IServiceProvider _serviceProvider;
    private readonly WebhookOptions _webhookOptions;
    private readonly IFileService _fileService;

    public MerchantService(
        IApplicationDbContext db,
        IUser user,
        IBotSecretGenerator secretGenerator,
        IServiceProvider serviceProvider,
        IOptions<WebhookOptions> webhookOptions,
        IFileService fileService)
    {
        _db = db;
        _user = user;
        _secretGenerator = secretGenerator;
        _serviceProvider = serviceProvider;
        _webhookOptions = webhookOptions.Value;
        _fileService = fileService;
    }

    #region Merchant Lifecycle

    public async Task<Result<Merchant>> CreateAsync(CreateMerchantRequest request, CancellationToken cancellationToken = default)
    {
        var profileResult = UserProfile.Create(
            Guid.NewGuid(),
            request.IdentityUserId,
            request.Profile.FirstName,
            request.Profile.LastName,
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
            request.MerchantName,
            request.Industry,
            profile,
            request.LogoUrl
        );

        if (!merchantResult.IsSuccess)
            return merchantResult.Errors;

        _db.Merchants.Add(merchantResult.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return merchantResult;
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

    public async Task<Result<ShopResponse>> AssignShopAsync(Guid merchantId, AssignShopRequest request, CancellationToken cancellationToken = default)
    {
        // Load aggregate root
        var merchant = await _db.Merchants.Include(x=> x.ShopSettings).FirstOrDefaultAsync(x=> x.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        if(merchant.ShopSettings is not null)
            return MerchantErrors.ShopAlreadyAssigned;

        string? uploadedImageUrl = null;
        if (request.Image is not null)
        {
            await using var stream = request.Image.OpenReadStream();
            var uploadResult = await _fileService.SaveFileAsync(stream, request.Image.FileName, "shop-images");
            if (!uploadResult.IsSuccess) return uploadResult.Errors;
            uploadedImageUrl = uploadResult.Value;
        }

        // Create ShopSettings via domain factory
        var settingsResult = ShopSettings.Create(
            id: Guid.NewGuid(),
            merchantId: merchantId,
            name: request.Name,
            currency: request.Currency,
            language: request.Language,
            address: request.Address,
            socialMediaLinks: request.SocialMediaLinks,
            imageUrl: uploadedImageUrl,
            taxRate: request.TaxRate
        );

        if (!settingsResult.IsSuccess) return settingsResult.Errors;
        var shop = settingsResult.Value;
        // Apply domain operation
        var assignResult = merchant.AssignShop(shop);
        if (!assignResult.IsSuccess) return assignResult.Errors;

        await _db.SaveChangesAsync(cancellationToken);
        return ShopResponse.FromDomain(shop);
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

        var webhookResult = await platformService.SetWebhookAsync(bot.Token, bot.WebhookUrl!, ct);
        if (!webhookResult.IsSuccess)
        {
            // Optionally mark bot state or log; return error to caller
            return BotErrors.WebhookRegistrationFailed;
        }

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
            .Include(m => m.Profile)
            .Include(m => m.Subscription)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null) return MerchantErrors.NotFound;

        return merchant.ToCurrentResponse();
    }

    #endregion
}
