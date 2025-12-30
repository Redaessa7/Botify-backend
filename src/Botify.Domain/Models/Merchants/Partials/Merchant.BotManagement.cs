using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        private readonly List<Bot> _bots = new();
        public IReadOnlyCollection<Bot> Bots => _bots.AsReadOnly();

        public Result<Bot> AddBot(Guid botId, string name, string token, string platform, string secret, string baseWebhookUrl)
        {
            var botResult = Bot.Create(botId, Id, name, token, platform, secret);
            if (!botResult.IsSuccess) return botResult.Errors;

            var bot = botResult.Value;
            var webhookUrl = $"{baseWebhookUrl.TrimEnd('/')}/{platform.ToLowerInvariant()}/{botId}?secret={secret}";
            var setWebhookResult = bot.SetWebhook(webhookUrl);
            if (!setWebhookResult.IsSuccess) return setWebhookResult.Errors;

            _bots.Add(bot);
            return bot;
        }
        public Result<Success> DeactivateBot(Guid botId)
        {
            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return BotErrors.BotNotFound;

            bot.Deactivate();
            DomainEventDispatcher.Raise(new BotDeactivated(Id, bot.Id));
            return Result.Success;
        }


        public Result<Success> ActivateBot(Guid botId)
        {
            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return BotErrors.BotNotFound;

            bot.Activate();
            DomainEventDispatcher.Raise(new BotActivated(Id, bot.Id, bot.Platform.ToString()));
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
