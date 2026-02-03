using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.SupportRequests;

public static class SupportRequestErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("SupportRequest.General.Id.Required", "SupportRequest ID is required.");

    public static Error MerchantIdRequired =>
        Error.Validation("SupportRequest.General.MerchantId.Required", "Merchant ID is required for the SupportRequest.");

    public static Error BotIdRequired =>
        Error.Validation("SupportRequest.General.BotId.Required", "Bot ID is required for the SupportRequest.");

    public static Error BotNameRequired =>
        Error.Validation("SupportRequest.General.BotName.Required", "Bot name is required.");

    public static Error PlatformRequired =>
        Error.Validation("SupportRequest.General.Platform.Required", "Platform is required.");

    public static Error TypeRequired =>
        Error.Validation("SupportRequest.General.Type.Required", "SupportRequest type is required.");

    public static Error TypeInvalid =>
        Error.Validation("SupportRequest.General.Type.Invalid", "SupportRequest type must be General, Complaint, Feedback, or Support.");

    public static Error ReasonRequired =>
        Error.Validation("SupportRequest.General.Reason.Required", "SupportRequest reason is required.");

    public static Error ReasonInvalid =>
        Error.Validation("SupportRequest.General.Reason.Invalid", "SupportRequest reason must be ClientRequest or AiEscalated.");

    public static Error CustomerNameRequired =>
        Error.Validation("SupportRequest.General.CustomerName.Required", "Customer name is required.");

    public static Error EmptyMessage =>
        Error.Validation("SupportRequest.General.Message.Empty", "Message cannot be empty.");

    public static Error MessageTooLong =>
        Error.Validation("SupportRequest.General.Message.TooLong", "Message cannot exceed 1000 characters.");

    // Status
    public static Error AlreadyResolved =>
        Error.Conflict("SupportRequest.Status.AlreadyResolved", "SupportRequest is already resolved.");

    public static Error NotResolved =>
        Error.Conflict("SupportRequest.Status.NotResolved", "SupportRequest is not resolved yet.");

    // Lookup
    public static Error NotFound =>
        Error.NotFound("SupportRequest.Lookup.NotFound", "The specified SupportRequest could not be found.");
}
