using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Questions;

public static class QuestionErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("Question.General.Id.Required", "Question ID is required.");

    public static Error MerchantIdRequired =>
        Error.Validation("Question.General.MerchantId.Required", "Merchant ID is required for the question.");

    public static Error BotIdRequired =>
        Error.Validation("Question.General.BotId.Required", "Bot ID is required for the question.");

    public static Error BotNameRequired =>
        Error.Validation("Question.General.BotName.Required", "Bot name is required.");

    public static Error PlatformRequired =>
        Error.Validation("Question.General.Platform.Required", "Platform is required.");

    public static Error TypeRequired =>
        Error.Validation("Question.General.Type.Required", "Question type is required.");

    public static Error TypeInvalid =>
        Error.Validation("Question.General.Type.Invalid", "Question type must be General, Complaint, Feedback, or Support.");

    public static Error CustomerNameRequired =>
        Error.Validation("Question.General.CustomerName.Required", "Customer name is required.");

    public static Error EmptyMessage =>
        Error.Validation("Question.General.Message.Empty", "Message cannot be empty.");

    public static Error MessageTooLong =>
        Error.Validation("Question.General.Message.TooLong", "Message cannot exceed 1000 characters.");

    // Status
    public static Error AlreadyResolved =>
        Error.Conflict("Question.Status.AlreadyResolved", "Question is already resolved.");

    public static Error NotResolved =>
        Error.Conflict("Question.Status.NotResolved", "Question is not resolved yet.");

    // Lookup
    public static Error NotFound =>
        Error.NotFound("Question.Lookup.NotFound", "The specified question could not be found.");
}
