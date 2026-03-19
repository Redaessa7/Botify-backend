namespace Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram.Models
{
    public sealed class TelegramDeleteWebhookResponse
    {
        public bool Ok { get; set; }
        public string? Description { get; set; }
        public int ErrorCode { get; set; }
    }
}
