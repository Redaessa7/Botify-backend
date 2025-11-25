using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Models.Merchants.BotConfigs;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.ShopsDetails;
using Botify.Domain.ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Domain.Models.Merchants
{
    public sealed class Merchant : AuditableEntity
    {
        public string FullName { get; private set; }
        public string ImageUrl { get; private set; }
        public MerchantStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? VerifiedAt { get; private set; }

        public MerchantSubscription Subscription { get; private set; }
        public ShopDetails? ShopDetails { get; private set; }

        private readonly List<BotConfig> _botConfigs = new();
        public IReadOnlyCollection<BotConfig> BotConfigs => _botConfigs.AsReadOnly();

        private Merchant() { }

        private Merchant(Guid id, string fullName, string imageUrl, Plan plan) : base(id)
        {
            FullName = fullName;
            ImageUrl = imageUrl;
            Status = MerchantStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            Subscription = MerchantSubscription.Start(Id, plan);
        }

        public static Result<Merchant> Create(Guid id, string fullName, string imageUrl, Plan plan)
        {
            if (id == Guid.Empty)
                id = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(fullName))
                return MerchantErrors.FullNameRequired;

            if (string.IsNullOrWhiteSpace(imageUrl))
                return MerchantErrors.ImageUrlRequired;

            return new Merchant(id, fullName, imageUrl, plan);
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
            var bot = _botConfigs.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return MerchantErrors.BotNotFound;

            bot.Enable();
            DomainEventDispatcher.Raise(new BotActivated(Id, bot.Id, bot.TargetPlatform.ToString()));
            return Result.Success;
        }

        public Result<Success> Suspend(string reason)
        {
            Status = MerchantStatus.Suspended;
            return Result.Success;
        }

        public Result<Success> AssignShopDetails(ShopDetails details)
        {
            if (Status != MerchantStatus.Approved)
                return MerchantErrors.NotApproved;

            ShopDetails = details;
            return Result.Success;
        }

        public Result<Success> AddBotConfig(BotConfig config)
        {
            if (ShopDetails == null || !ShopDetails.IsComplete())
                return MerchantErrors.ShopIncomplete;

            if (!Subscription.IsActive())
                return MerchantErrors.SubscriptionExpired;

            if (!CanCreateBot())
                return MerchantErrors.PlanLimitReached;

            if (_botConfigs.Any(b => b.Enabled && b.TargetPlatform == config.TargetPlatform))
                return MerchantErrors.BotPlatformConflict;

            _botConfigs.Add(config);
            return Result.Success;
        }

        public Result<Success> DeactivateBot(Guid botId)
        {
            var bot = _botConfigs.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return MerchantErrors.BotNotFound;

            bot.Disable();
            return Result.Success;
        }

        public bool CanCreateBot()
        {
            return Subscription.CurrentPlan switch
            {
                Plan.Free => _botConfigs.Count < 1,
                Plan.Pro => _botConfigs.Count < 3,
                Plan.Enterprise => true,
                _ => false
            };
        }
    }
}
