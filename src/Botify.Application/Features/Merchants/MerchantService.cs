using Botify.Application.Common.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Services.Merchants.Requests;
using Botify.Application.Services.Merchants.Responses;
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

    public MerchantService(
        IApplicationDbContext db,
        IUser user,
        IBotSecretGenerator secretGenerator,
        IServiceProvider serviceProvider,
        IOptions<WebhookOptions> webhookOptions)
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

    public async Task<Result<Success>> AssignShopAsync(Guid merchantId, ShopSettings settings, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var result = merchant.AssignShop(settings);
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion

    #region Bot Management

    public async Task<Result<Success>> CreateBotAsync(Guid merchantId, CreateBotRequest request, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.Include(m => m.Bots).FirstOrDefaultAsync(m => m.Id == merchantId, ct);
        if (merchant is null) return MerchantErrors.NotFound;

        // Resolve platform service via KeyedServices
        var platformService = _serviceProvider.GetKeyedService<IBotPlatformService>(request.Platform.ToLowerInvariant());
        if (platformService is null) return BotErrors.PlatformInvalid;

        // Validate token externally
        var tokenCheck = await platformService.ValidateTokenAsync(request.ApiToken, ct);
        if (!tokenCheck.IsSuccess) return tokenCheck.Errors;

        // Generate secret
        var secret = _secretGenerator.Generate();

        // Add bot via aggregate root
        var botResult = merchant.AddBot(Guid.NewGuid(), request.Name, request.ApiToken, request.Platform, secret, _webhookOptions.BaseUrl);
        if (!botResult.IsSuccess) return botResult.Errors;

        var bot = botResult.Value;
        _db.Bots.Add(bot);
        await _db.SaveChangesAsync(ct);

        // Register webhook with external platform
        var webhookResult = await platformService.SetWebhookAsync(bot.Token, bot.WebhookUrl!, ct);
        if (!webhookResult.IsSuccess) return BotErrors.WebhookRegistrationFailed;

        return Result.Success;
    }
    public async Task<Result<List<GetListBotResponse>>> GetBotsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .Include(m => m.Bots)
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (merchant is null)
            return MerchantErrors.NotFound;

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
