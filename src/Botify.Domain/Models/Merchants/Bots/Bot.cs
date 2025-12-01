using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Domain.Models.Merchants.Bots
{
    public sealed class Bot : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public string Token { get; private set; }
        public string Secret { get; private set; }
        public BotPlatform TargetPlatform { get; private set; }
        public bool Enabled { get; private set; }
        public bool PlatformVerified { get; private set; }
        public bool IsPrimary { get; set; }
        public string? WebhookUrl { get; private set; }

        private Bot() { }
        private Bot(Guid id, Guid merchantId, string token, string secret, BotPlatform platform) : base(id)
        {
            MerchantId = merchantId;
            Token = token;
            Secret = secret;
            TargetPlatform = platform;
            Enabled = false;
            PlatformVerified = false;
        }

        public static Bot Create(Guid id,  Guid merchantId, string token, string secret, BotPlatform platform)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Token and secret are required.");

            return new Bot(id, merchantId, token, secret, platform);
        }

        public void VerifyPlatform()
        {
            PlatformVerified = true;
        }

        public void Enable()
        {
            if (!PlatformVerified)
                throw new InvalidOperationException("Platform must be verified before enabling bot.");

            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void SetWebhook(string webhookUrl)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
                throw new ArgumentException("Webhook URL cannot be empty.");

            WebhookUrl = webhookUrl;
        }
    }

}
