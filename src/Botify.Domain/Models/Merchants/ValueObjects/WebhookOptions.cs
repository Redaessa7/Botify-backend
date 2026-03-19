namespace Botify.Domain.Models.Merchants.ValueObjects
{
    public sealed class WebhookOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string WebhookRoute { get; } = "/api/webhook/bot/";
        public string GetWebhookUrl => BaseUrl! + WebhookRoute;
    }
}
