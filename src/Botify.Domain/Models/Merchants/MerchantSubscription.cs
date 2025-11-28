using Botify.Domain.Models.Merchants.Enums;

namespace Botify.Domain.Models.Merchants
{
    // MerchantSubscription
    public sealed class MerchantSubscription
    {
        public Guid MerchantId { get; private set; }
        public Plan CurrentPlan { get; private set; }
        public DateTime ExpiryDate { get; private set; }

        private MerchantSubscription() {}
        private MerchantSubscription(Guid merchantId, Plan plan, DateTime expiryDate)
        {
            MerchantId = merchantId;
            CurrentPlan = plan;
            ExpiryDate = expiryDate;
        }

        public static MerchantSubscription Start(Guid merchantId, Plan plan)
        {
            return new MerchantSubscription(merchantId, plan, DateTime.UtcNow.AddMonths(1));
        }

        public void Renew(int months)
        {
            ExpiryDate = ExpiryDate < DateTime.UtcNow
                ? DateTime.UtcNow.AddMonths(months)
                : ExpiryDate.AddMonths(months);
        }

        public void Upgrade(Plan newPlan)
        {
            CurrentPlan = newPlan;
        }

        public bool IsActive() => ExpiryDate > DateTime.UtcNow;
    }
}