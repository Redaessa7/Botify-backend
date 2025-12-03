namespace Botify.Application.Services.Merchants
{
    public partial class MerchantService
    {
        public sealed record CreateBotRequest(string Name, string ApiToken, string Platform);
    }
}
