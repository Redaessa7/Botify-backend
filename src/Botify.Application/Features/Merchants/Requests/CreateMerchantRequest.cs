namespace Botify.Application.Services.Merchants.Requests
{
    public class CreateMerchantRequest
    {
        public CreateMerchantRequest(
            string identityUserId,
            CreateUserProfileRequest profile)
        {
            IdentityUserId = identityUserId;
            Profile = profile;
        }

        public string IdentityUserId { get; }
        public CreateUserProfileRequest Profile { get; }
    }

}
