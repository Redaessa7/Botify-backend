using Botify.Domain.Models.SupportRequests;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        private readonly List<SupportRequest> _questions = new();
        public IReadOnlyCollection<SupportRequest> Questions => _questions.AsReadOnly();

        public Result<SupportRequest> AddQuestion(SupportRequest question)
        {
            _questions.Add(question);
            return question;
        }
    }
}
