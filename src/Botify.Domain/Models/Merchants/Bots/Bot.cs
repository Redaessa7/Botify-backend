using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Bots;

public sealed class Bot : AuditableEntity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Token { get; private set; }
    public string Secret { get; private set; }
    public BotPlatform TargetPlatform { get; private set; }
    public bool Enabled { get; private set; }
    public bool PlatformVerified { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? WebhookUrl { get; private set; }

    private Bot() { }

    private Bot(Guid id, Guid merchantId, string token, string name, BotPlatform platform) : base(id)
    {
        MerchantId = merchantId;
        Name = name;
        Token = token;
        Secret = "for now"; // TODO: replace with secure generation
        TargetPlatform = platform;
        Enabled = false;
        PlatformVerified = false;
    }

    public static Result<Bot> Create(Guid id, Guid merchantId, string token, string name, string platform)
    {
        if (id == Guid.Empty)
            return BotErrors.IdRequired;

        if (merchantId == Guid.Empty)
            return BotErrors.MerchantIdRequired;

        if (string.IsNullOrWhiteSpace(name))
            return BotErrors.NameRequired;

        if (name.Length > 100)
            return BotErrors.NameTooLong;

        if (string.IsNullOrWhiteSpace(token))
            return BotErrors.TokenRequired;

        var platformResult = BotPlatform.From(platform);
        if (!platformResult.IsSuccess)
            return platformResult.Errors;

        return new Bot(id, merchantId, token, name, platformResult.Value);
    }


    public Result<Success> VerifyPlatform()
    {
        PlatformVerified = true;
        return Result.Success;
    }

    public Result<Success> Enable()
    {
        if (!PlatformVerified)
            return BotErrors.PlatformNotVerified;

        if (Enabled)
            return BotErrors.AlreadyEnabled;

        Enabled = true;
        return Result.Success;
    }

    public Result<Success> Disable()
    {
        if (!Enabled)
            return BotErrors.AlreadyDisabled;

        Enabled = false;
        return Result.Success;
    }

    public Result<Success> SetWebhook(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return BotErrors.WebhookUrlRequired;

        if (!Uri.IsWellFormedUriString(webhookUrl, UriKind.Absolute))
            return BotErrors.WebhookUrlInvalid;

        WebhookUrl = webhookUrl;
        return Result.Success;
    }
}
