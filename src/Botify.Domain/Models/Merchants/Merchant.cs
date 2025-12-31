using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant : AuditableEntity
    {
        public MerchantStatus Status { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public Guid UserProfileId { get; private set; }
        public UserProfile Profile { get; private set; }
        public MerchantSubscription Subscription { get; private set; }

        private Merchant() { }

        private Merchant(Guid id, UserProfile userProfile, Plan plan = Plan.Free)
            : base(id)
        {
            UserProfileId = userProfile.Id;
            Status = MerchantStatus.Pending;
            Subscription = MerchantSubscription.Start(Id, plan);
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
    }
}
