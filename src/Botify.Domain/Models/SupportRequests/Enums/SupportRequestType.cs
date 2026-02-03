using Botify.Domain.Models.SupportRequests;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.SupportRequests.Enums
{
    public enum SupportRequestType
    {
        GENERAL,
        COMPLAINT,
        FEEDBACK,
        SUPPORT
    }

    public static class QuestionTypeExtensions
    {
        public static Result<SupportRequestType> From(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return SupportRequestErrors.TypeRequired;

            return type.ToUpperInvariant() switch
            {
                "GENERAL" => SupportRequestType.GENERAL,
                "COMPLAINT" => SupportRequestType.COMPLAINT,
                "FEEDBACK" => SupportRequestType.FEEDBACK,
                "SUPPORT" => SupportRequestType.SUPPORT,
                _ => SupportRequestErrors.TypeInvalid
            };
        }

        public static string AsString(this SupportRequestType type) =>
            type.ToString();
    }
}
