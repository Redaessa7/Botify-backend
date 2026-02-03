using Botify.Domain.Models.SupportRequests;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.SupportRequests.Enums
{
    public enum RequestReason
    {
        CLIENT_REQUEST,
        AI_ESCALATED
    }

    public static class RequestReasonExtensions
    {
        public static Result<RequestReason> From(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return SupportRequestErrors.ReasonRequired;

            return reason.ToUpperInvariant() switch
            {
                "CLIENT_REQUEST" => RequestReason.CLIENT_REQUEST,
                "AI_ESCALATED" => RequestReason.AI_ESCALATED,
                _ => SupportRequestErrors.ReasonInvalid
            };
        }

        public static string AsString(this RequestReason reason) =>
            reason.ToString();
    }
}
