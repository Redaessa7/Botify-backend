using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Services.Merchants
{


        public sealed record CreateBotRequest(string Name, string ApiToken, BotPlatform Platform);
    
}
