namespace Botify.Domain.Models.Bots.ValueObjects
{
    public sealed class BotDailyStats
    {
        public Guid BotId { get; private set; }
        public DateTime Date { get; private set; }
        public int MessageCount { get; private set; }
        public int OrderCount { get; private set; }

        private BotDailyStats() { }

        public static BotDailyStats Create(Guid botId, DateTime date)
            => new BotDailyStats
            {
                BotId = botId,
                Date = date.Date,
                MessageCount = 0,
                OrderCount = 0
            };

        public void IncrementMessages() => MessageCount++;
        public void IncrementOrders() => OrderCount++;
    }
}
