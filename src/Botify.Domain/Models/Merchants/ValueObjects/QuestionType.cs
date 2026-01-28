using Botify.Domain.Models.Questions;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.ValueObjects
{
    public enum QuestionType
    {
        GENERAL,
        COMPLAINT,
        FEEDBACK,
        SUPPORT
    }

    public static class QuestionTypeExtensions
    {
        public static Result<QuestionType> From(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return QuestionErrors.TypeRequired;

            return type.ToUpperInvariant() switch
            {
                "GENERAL" => QuestionType.GENERAL,
                "COMPLAINT" => QuestionType.COMPLAINT,
                "FEEDBACK" => QuestionType.FEEDBACK,
                "SUPPORT" => QuestionType.SUPPORT,
                _ => QuestionErrors.TypeInvalid
            };
        }

        public static string AsString(this QuestionType type) =>
            type.ToString();
    }
}
