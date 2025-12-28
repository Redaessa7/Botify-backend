namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class ProductStatsResponse
    {
        public int Total { get; init; }
        public int Active { get; init; }
        public int OutOfStock { get; init; }
    }
}
