using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.BotConfigs;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ShopsDetails;
using Botify.Domain.ResultPattern;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Botify.Application.Services.Merchants
{
    public interface IMerchantService
    {
        Task<Result<Merchant>> CreateAsync(string fullName,string userId,string imageUrl, Plan plan, CancellationToken cancellationToken = default);
        Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default);
        Task<Result<Success>> AssignShopDetailsAsync(Guid merchantId, ShopDetails details, CancellationToken cancellationToken = default);
        Task<Result<Success>> AddBotConfigAsync(Guid merchantId, BotConfig config, CancellationToken cancellationToken = default);
        Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
        Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
    }
}
