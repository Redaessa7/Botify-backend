using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.ResultPattern;
using Botify.Domain.Models.Bots.ValueObjects;

namespace Botify.Domain.Models.Bots;

public sealed class Bot : AuditableEntity
{
    public Guid MerchantId { get; private set; }
    public string Name { get; private set; }
    public string Token { get; private set; }
    public string Secret { get; private set; }
    public BotPlatform Platform { get; private set; }
    public bool Active { get; private set; }
    public bool PlatformVerified { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? WebhookUrl { get; private set; }

    // 👇 New column
    public DateTime? LastPing { get; private set; }

    private readonly List<SupportRequest> _questions = [];
    public IReadOnlyCollection<SupportRequest> Questions => _questions.AsReadOnly();

    private readonly List<BotDailyStats> _dailyStats = [];
    public IReadOnlyCollection<BotDailyStats> DailyStats => _dailyStats.AsReadOnly();

    private Bot() { }

    private Bot(Guid id, Guid merchantId, string name, string token, BotPlatform platform, string secret) : base(id)
    {
        MerchantId = merchantId;
        Name = name;
        Token = token;
        Platform = platform;
        Secret = secret;
        Active = true;
        PlatformVerified = false;
        IsPrimary = false;
    }

    internal static Result<Bot> Create(Guid id, Guid merchantId, string name, string token, BotPlatform platform, string secret)
    {
        if (id == Guid.Empty) return BotErrors.IdRequired;
        if (merchantId == Guid.Empty) return BotErrors.MerchantIdRequired;
        if (string.IsNullOrWhiteSpace(name)) return BotErrors.NameRequired;
        if (name.Length > 100) return BotErrors.NameTooLong;
        if (string.IsNullOrWhiteSpace(token)) return BotErrors.TokenRequired;
        if (string.IsNullOrWhiteSpace(secret)) return BotErrors.SecretRequired;

        return new Bot(id, merchantId, name.Trim(), token.Trim(), platform, secret.Trim());
    }

    internal Result<Success> SetWebhook(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl)) return BotErrors.WebhookUrlRequired;
        if (!Uri.IsWellFormedUriString(webhookUrl, UriKind.Absolute)) return BotErrors.WebhookUrlInvalid;

        WebhookUrl = webhookUrl;
        return Result.Success;
    }

    public Result<Success> VerifyPlatform()
    {
        PlatformVerified = true;
        return Result.Success;
    }

    public Result<Success> Activate()
    {
        if (Active) return BotErrors.AlreadyActive;
        Active = true;
        return Result.Success;
    }

    public Result<Success> Deactivate()
    {
        if (!Active) return BotErrors.AlreadyInactive;
        Active = false;
        return Result.Success;
    }

    public Result<Success> UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return BotErrors.NameRequired;
        if (newName.Length > 100) return BotErrors.NameTooLong;
        Name = newName.Trim();
        return Result.Success;
    }

    // -------------------- Stats Methods --------------------
    public void RecordMessage(DateTime date)
    {
        var stat = _dailyStats.FirstOrDefault(s => s.Date == date.Date);
        if (stat is null)
        {
            stat = BotDailyStats.Create(Id, date);
            _dailyStats.Add(stat);
        }
        stat.IncrementMessages();
    }

    public void RecordOrder(DateTime date)
    {
        var stat = _dailyStats.FirstOrDefault(s => s.Date == date.Date);
        if (stat is null)
        {
            stat = BotDailyStats.Create(Id, date);
            _dailyStats.Add(stat);
        }
        stat.IncrementOrders();
    }

    // -------------------- New Method --------------------
    public void UpdateLastPing()
    {
        LastPing = DateTime.UtcNow;
    }
}
