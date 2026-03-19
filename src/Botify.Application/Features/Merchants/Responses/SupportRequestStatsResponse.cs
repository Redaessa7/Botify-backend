namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class SupportRequestStatsResponse
    {
        public int Total { get; init; }
        public int Pending { get; init; }

        public int ClientRequests { get; init; }
        public int AIEscalated { get; init; }
    }
}
