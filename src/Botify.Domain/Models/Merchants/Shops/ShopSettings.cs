namespace Botify.Domain.Models.Merchants.Shops
{
    public sealed class ShopSettings : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public string Name { get; private set; }
        public string Currency { get; private set; }
        public string Language { get; private set; }
        public string Address { get; private set; }
        public string? TelegramLink { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal? TaxRate { get; private set; }

        private ShopSettings() { }
        private ShopSettings(Guid id, Guid merchantId, string name, string currency, string language, string address) : base(id)
        {
            MerchantId = merchantId;
            Name = name;
            Currency = currency;
            Language = language;
            Address = address;
        }

        public static ShopSettings Create(Guid id, Guid merchantId, string name, string currency, string language, string address)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(currency) ||
                string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("All required fields must be filled.");

            return new ShopSettings(id, merchantId, name, currency, language, address);
        }

        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Currency)
                && !string.IsNullOrWhiteSpace(Language)
                && !string.IsNullOrWhiteSpace(Address);
        }

        public void UpdateDetails(string name, string currency, string language, string address)
        {
            Name = name;
            Currency = currency;
            Language = language;
            Address = address;
        }
    }

}
