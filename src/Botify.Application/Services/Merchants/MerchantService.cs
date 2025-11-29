using Botify.Application.Common.Interfaces;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.BotConfigs;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ShopsDetails;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Services.Merchants
{
    public class MerchantService : IMerchantService
    {
        private readonly IApplicationDbContext _db;

        public MerchantService(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Merchant>> CreateAsync(string fullName, string UserId,string imageUrl, Plan plan, CancellationToken cancellationToken = default)
        {
            var merchantResult = Merchant.Create(Guid.NewGuid(),UserId, fullName, imageUrl, plan);
            if (!merchantResult.IsSuccess)
                return merchantResult;

            _db.Merchants.Add(merchantResult.Value);
            await _db.SaveChangesAsync(cancellationToken);

            return merchantResult;
        }

        public async Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FirstOrDefaultAsync(x => x.Id == merchantId,cancellationToken);
            if (merchant is null)
                return MerchantErrors.NotFound; // you may add MerchantErrors.NotFound

            var result = merchant.Approve();
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
            if (merchant is null)
                return MerchantErrors.BotNotFound;

            var result = merchant.Suspend(reason);
            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> AssignShopDetailsAsync(Guid merchantId, ShopDetails details, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants.FindAsync(new object[] { merchantId }, cancellationToken);
            if (merchant is null)
                return MerchantErrors.BotNotFound;

            var result = merchant.AssignShopDetails(details);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> AddBotConfigAsync(Guid merchantId, BotConfig config, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.BotConfigs)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return MerchantErrors.BotNotFound;

            var result = merchant.AddBotConfig(config);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.BotConfigs)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return MerchantErrors.BotNotFound;

            var result = merchant.ActivateBot(botId);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.BotConfigs)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

            if (merchant is null)
                return MerchantErrors.BotNotFound;

            var result = merchant.DeactivateBot(botId);
            if (!result.IsSuccess) return result;

            await _db.SaveChangesAsync(cancellationToken);
            return result;
        }
    }
}
