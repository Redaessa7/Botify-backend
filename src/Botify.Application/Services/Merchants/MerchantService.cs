using Botify.Application.Common.Interfaces;
using Botify.Application.Services.Merchants.Requests;
using Botify.Application.Services.Merchants.Responses;
using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Services.Merchants
{
    public class MerchantService : IMerchantService
    {
        private readonly IApplicationDbContext _db;
        private readonly IUser _user;
        public MerchantService(IApplicationDbContext db, IUser user)
        {
            _db = db;
            _user = user;
        }

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

            var merchant = merchantResult.Value;

            _db.Merchants.Add(merchant);

            await _db.SaveChangesAsync(cancellationToken);

            return merchantResult;
        }


        public async Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FirstOrDefaultAsync(x => x.Id == merchantId,cancellationToken);
            if (merchant is null)
                return MerchantErrors.NotFound; 

            var result = merchant.Approve();
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
            if (merchant is null)
                return BotErrors.BotNotFound;

            var result = merchant.Suspend(reason);
            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> AssignShopAsync(Guid merchantId, ShopSettings settings, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
            if (merchant is null)
                return BotErrors.BotNotFound;

            var result = merchant.AssignShop(settings);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> AddBotAsync(Guid merchantId, Bot bot, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.Bots)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return BotErrors.BotNotFound;

            var result = merchant.AddBot(bot);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.Bots)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return BotErrors.BotNotFound;

            var result = merchant.ActivateBot(botId);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.Bots)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return MerchantErrors.NotFound;

            var result = merchant.DeactivateBot(botId);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<CurrentMerchantResponse>> GetCurrentMerchantAsync(CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.Profile)
                .Include(m => m.Subscription)
                .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

            if (merchant is null)
            {
                return MerchantErrors.NotFound;
            }

            return merchant.ToCurrentResponse();
        }
    }
}
