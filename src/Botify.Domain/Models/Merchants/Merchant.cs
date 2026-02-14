using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Identity;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Products;
using Botify.Domain.Models.Shops;
using Botify.Domain.ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant : AuditableEntity
    {
        public MerchantStatus Status { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public Guid UserProfileId { get; private set; }
        public UserProfile Profile { get; private set; }
        public MerchantSubscription Subscription { get; private set; }

        private readonly List<Customer> _customers = new();
        public IReadOnlyCollection<Customer> Customers => _customers.AsReadOnly();


        public QuickActions Preferences { get; private set; } = QuickActions.None;

        public bool IsServiceActivated =>
            _bots.Any(b => b.Active) &&
            ShopSettings is not null &&
            AIConfiguration is not null;

        /// <summary>
        /// Returns the last time the merchant modified any significant entity (bots, orders, products, shop, AI config).
        /// Falls back to CreatedAtUtc if no modifications exist.
        /// </summary>
        public DateTime LastUpdated
        {
            get
            {
                var timestamps = new List<DateTime>();

                if (_bots.Any())
                    timestamps.Add(_bots.Max(b => b.LastModifiedUtc));

                if (_orders.Any())
                    timestamps.Add(_orders.Max(o => o.LastModifiedUtc));

                if (_products.Any())
                    timestamps.Add(_products.Max(p => p.LastModifiedUtc));

                if (ShopSettings is not null)
                    timestamps.Add(ShopSettings.LastModifiedUtc);

                if (AIConfiguration is not null)
                    timestamps.Add(AIConfiguration.LastModifiedUtc);

                return timestamps.Any() ? timestamps.Max() : CreatedAtUtc;
            }
        }

        private Merchant() { }

        private Merchant(Guid id, UserProfile userProfile, Plan plan = Plan.Free)
            : base(id)
        {
            UserProfileId = userProfile.Id;
            Status = MerchantStatus.Pending;
            Subscription = MerchantSubscription.Start(Id, plan);
            Preferences = QuickActions.None;
        }

        public static Result<Merchant> Create(Guid id, UserProfile userProfile)
        {
            if (id == Guid.Empty)
                return MerchantErrors.IdRequired;

            if (userProfile is null)
                return MerchantErrors.UserProfileRequired;

            return new Merchant(id, userProfile);
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

        public Result<Success> Suspend(string reason)
        {
            Status = MerchantStatus.Suspended;
            return Result.Success;
        }

        public Result<Success> UpdatePreferences(QuickActions actions)
        {
            Preferences = actions;
            return Result.Success;
        }

        public Result<Updated> UpdateBasicInfo(string firstName, string lastName, string? bio, string? profileImageUrl)
        {
            var result = Profile.UpdateBasicInfo(firstName, lastName, bio, profileImageUrl);

            if(result.IsError)
                return result.Errors;

            return result;
        }

        /// <summary>
        /// Determines the current onboarding step based on merchant state.
        /// </summary>
        public int GetOnboardingStep()
        {
            if (!_bots.Any())
                return 1;

            if (AIConfiguration is null)
                return 2;

            if (ShopSettings is null)
                return 3;

            if (_products.Any())
                return 4;

            return -1; // fallback if none match
        }
    }
}
