using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.ResultPattern;
using static Botify.Application.Features.Merchants.Services.MerchantService;

namespace Botify.Application.Features.Merchants.Interfaces
{
    public interface IMerchantService
    {
       Task<Result<bool>> CheckShopExistsAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<ShopResponse>> GetShopAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<List<GetListBotResponse>>> GetBotsAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<BotResponse>> GetBotByIdAsync(Guid merchantId, Guid botId, CancellationToken ct);
        Task<Result<CurrentMerchantResponse>> GetCurrentMerchantAsync(CancellationToken cancellationToken = default);
        Task<Result<Merchant>> CreateAsync(CreateMerchantRequest request,CancellationToken cancellationToken = default);
        Task<Result<Success>> ApproveAsync(Guid merchantId, CancellationToken cancellationToken = default);
        Task<Result<Success>> SuspendAsync(Guid merchantId, string reason, CancellationToken cancellationToken = default);
        Task<Result<ShopResponse>> SetShopAsync(Guid merchantId, SetShopRequest request, CancellationToken cancellationToken = default);
        Task<Result<BotResponse>> CreateBotAsync(Guid merchantId, CreateBotRequest request, CancellationToken cancellationToken = default);
        Task<Result<Success>> ActivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
        Task<Result<Success>> DeactivateBotAsync(Guid merchantId, Guid botId, CancellationToken cancellationToken = default);
    }
}
