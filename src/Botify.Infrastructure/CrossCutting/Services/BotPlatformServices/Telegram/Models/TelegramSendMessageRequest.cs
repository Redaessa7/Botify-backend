using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram.Models;
    public sealed record TelegramSendMessageRequest(
        [property: JsonPropertyName("chat_id")] long ChatId,
        [property: JsonPropertyName("text")] string Text
    );

public sealed class TelegramSendMessageResponse
{
    public bool Ok { get; set; }
}
