using System.Text.Json.Serialization;

namespace Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram.Models
{
    public sealed class TelegramGetMeResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public TelegramUser? Result { get; set; }
    }

    public sealed class TelegramUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("is_bot")]
        public bool IsBot { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }

        [JsonPropertyName("can_join_groups")]
        public bool CanJoinGroups { get; set; }

        [JsonPropertyName("can_read_all_group_messages")]
        public bool CanReadAllGroupMessages { get; set; }

        [JsonPropertyName("supports_inline_queries")]
        public bool SupportsInlineQueries { get; set; }

        [JsonPropertyName("can_connect_to_business")]
        public bool CanConnectToBusiness { get; set; }

        [JsonPropertyName("has_main_web_app")]
        public bool HasMainWebApp { get; set; }
    }
}
