namespace Botify.Application.Features.Webhooks;

public record TelegramRequest(Guid BotId, string SecretKey,string UserMessage);