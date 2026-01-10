namespace Botify.Application.Features.Orders.Responses
{
    public sealed record OrderStatsResponse(
        int Total,
        int Pending,
        int Preparing,
        int Delivered
    );
}
