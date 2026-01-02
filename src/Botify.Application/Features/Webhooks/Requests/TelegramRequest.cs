namespace Botify.Application.Features.Webhooks.Requests;

public record TelegramRequest(Guid BotId, string SecretKey,string UserMessage);