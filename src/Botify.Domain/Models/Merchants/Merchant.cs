using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Identity;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Enums;
using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed class Merchant : AuditableEntity
    {
        public MerchantStatus Status { get; private set; }
        public DateTime? VerifiedAt { get; private set; }
        public Guid UserProfileId { get; private set; }
        public UserProfile Profile { get; private set; }

        public MerchantSubscription Subscription { get; private set; }
        public ShopSettings? ShopSettings { get; private set; }

        public AIConfiguration  AIConfiguration { get; private set; }

        private readonly List<Bot> _bots = new();
        private readonly List<Product> _products = new();
        private readonly List<Question> _questions = [];
        public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();
        public IReadOnlyCollection<Bot> Bots => _bots.AsReadOnly();

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

        #region Merchant Lifecycle

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

        public Result<Success> AssignShop(ShopSettings settings)
        {
            ShopSettings = settings;
            return Result.Success;
        }

        #endregion

        #region Bot Management

        private string BuildWebhookUrl(Guid botId, string secret, string platform, string baseWebhookUrl)
        {
            var webhookUrl = $"{baseWebhookUrl.TrimEnd('/')}/{platform.ToLowerInvariant()}/{botId}?secret={secret}";
            return webhookUrl;
        }

        public Result<Bot> AddBot(Guid botId, string name, string token, string platform, string secret, string baseWebhookUrl)
        {
            var botResult = Bot.Create(botId, Id, name, token, platform, secret);
            if (!botResult.IsSuccess) return botResult.Errors;

            var bot = botResult.Value;

            var webhookUrl = BuildWebhookUrl(botId, secret, platform, baseWebhookUrl);
            var setWebhookResult = bot.SetWebhook(webhookUrl);
            if (!setWebhookResult.IsSuccess) return setWebhookResult.Errors;

            _bots.Add(bot);
            return bot;
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

        public Result<Success> DeactivateBot(Guid botId)
        {
            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot == null)
                return BotErrors.BotNotFound;

            bot.Deactivate();
            DomainEventDispatcher.Raise(new BotDeactivated(Id, bot.Id));
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

        #endregion

        #region Product Management

        public Result<Product> AddProduct(Product product)
        {
            _products.Add(product);
            return product;
        }

        public Result<Success> ActivateProduct(Guid productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return ProductErrors.NotFound;

            product.Activate();
            return Result.Success;
        }

        public Result<Success> DeactivateProduct(Guid productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return ProductErrors.NotFound;

            product.Deactivate();
            return Result.Success;
        }

        #endregion

        #region Question Management
        public Result<Question> AddQuestion(Question question)
        {
            _questions.Add(question);
            return question;
        }
        #endregion
    }
}
