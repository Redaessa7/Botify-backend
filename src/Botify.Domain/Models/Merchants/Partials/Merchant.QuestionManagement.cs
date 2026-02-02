using Botify.Domain.Models.Questions;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        private readonly List<Question> _questions = new();
        public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

        public Result<Question> AddQuestion(Question question)
        {
            _questions.Add(question);
            return question;
        }
    }
}
