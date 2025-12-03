using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed class Merchant : AuditableEntity
    {
        public string MerchantName { get; private set; }
        public string Industry { get; private set; }
        public string? LogoUrl { get; private set; }
        public MerchantStatus Status { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public Guid UserProfileId { get; private set; }
        public UserProfile Profile { get; private set; }

        public MerchantSubscription Subscription { get; private set; }
        public ShopSettings? ShopSettings { get; private set; }

        private readonly List<Bot> _bots = new();
        public IReadOnlyCollection<Bot> Bots => _bots.AsReadOnly();

        private Merchant() { }

        private Merchant(Guid id, string merchantName, string industry, UserProfile userProfile, string? logoUrl, Plan plan = Plan.Free)
            : base(id)
        {
            UserProfileId = userProfile.Id;
            MerchantName = merchantName;
            Industry = industry;
            LogoUrl = logoUrl;
            Status = MerchantStatus.Pending;
            Subscription = MerchantSubscription.Start(Id, plan);
        }

        public static Result<Merchant> Create(Guid id, string merchantName, string industry, UserProfile userProfile, string? logoUrl)
        {
            if (id == Guid.Empty)
                return MerchantErrors.IdRequired;

            if (string.IsNullOrWhiteSpace(merchantName))
                return MerchantErrors.MerchantNameRequired;

            if (string.IsNullOrWhiteSpace(industry))
                return MerchantErrors.IndustryRequired;

            if (userProfile is null)
                return MerchantErrors.UserProfileRequired;

            return new Merchant(id, merchantName, industry, userProfile, logoUrl);
        }


        public Result<Success> Approve()
        {
            if (Status != MerchantStatus.Pending)
                return MerchantErrors.NotPending;

            Status = MerchantStatus.Approved;
            VerifiedAt = DateTime.UtcNow;

            DomainEventDispatcher.Raise(new MerchantApproved(Id));
            return Result.Success;
        }

        public Result<Success> ActivateBot(Guid botId)
        {
            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return BotErrors.BotNotFound;

            bot.Enable();
            DomainEventDispatcher.Raise(new BotActivated(Id, bot.Id, bot.TargetPlatform.ToString()));
            return Result.Success;
        }

        public Result<Success> Suspend(string reason)
        {
            Status = MerchantStatus.Suspended;
            return Result.Success;
        }

        public Result<Success> AssignShop(ShopSettings settings)
        {
            if (Status != MerchantStatus.Approved)
                return MerchantErrors.NotApproved;

            ShopSettings = settings;
            return Result.Success;
        }

        public Result<Success> AddBot(Bot bot)
        {
            //if (ShopSettings == null || !ShopSettings.IsComplete())
            //    return ShopErrors.ShopIncomplete;

            //if (!Subscription.IsActive())
            //    return MerchantErrors.SubscriptionExpired;

            //if (!CanCreateBot())
            //    return MerchantErrors.PlanLimitReached;

            //if (Bots.Any(b => b.Enabled && b.TargetPlatform == bot.TargetPlatform))
            //    return BotErrors.BotPlatformConflict;

            _bots.Add(bot);
            return Result.Success;
        }

        public Result<Success> DeactivateBot(Guid botId)
        {
            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return BotErrors.BotNotFound;

            bot.Disable();
            return Result.Success;
        }
        
        public bool CanCreateBot()
        {
            return Subscription.CurrentPlan switch
            {
                Plan.Free => _bots.Count <= 1,
                Plan.Pro => _bots.Count < 3,
                Plan.Enterprise => true,
                _ => false
            };
        }
    }
}
