using Botify.Application.Services.Merchants.Requests;
using Botify.Application.Services.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.ResultPattern;
using static Botify.Application.Services.Merchants.MerchantService;

namespace Botify.Application.Services.Merchants
{
    public interface IMerchantService
    {
        Task<Result<CurrentMerchantResponse>> GetCurrentMerchantAsync(CancellationToken cancellationToken = default);
        Task<Result<Merchant>> CreateAsync(CreateMerchantRequest request,CancellationToken cancellationToken = default);
        Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default);
        Task<Result<Success>> AssignShopAsync(Guid merchantId, ShopSettings settings, CancellationToken cancellationToken = default);
        Task<Result<Success>> CreateBotAsync(Guid merchantId, CreateBotRequest request, CancellationToken cancellationToken = default);
        Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
        Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
    }
}
