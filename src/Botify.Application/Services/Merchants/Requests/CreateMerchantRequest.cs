namespace Botify.Application.Services.Merchants.Requests
{
    public class CreateMerchantRequest
    {
        public CreateMerchantRequest(
            string identityUserId,
            string merchantName,
            string industry,
            string? logoUrl,
            CreateUserProfileRequest profile)
        {
            IdentityUserId = identityUserId;
            MerchantName = merchantName;
            Industry = industry;
            LogoUrl = logoUrl;
            Profile = profile;
        }

        public string IdentityUserId { get; }
        public string MerchantName { get; }
        public string Industry { get; }
        public string? LogoUrl { get; }
        public CreateUserProfileRequest Profile { get; }
    }

}
