using System;
using System.Text;
using System.Text.Json;

namespace Botify.Application.CrossCutting.Helpers
{
    public static class CursorHelper
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Object that holds both Id and CreatedAt
        public record CursorPayload(Guid Id, DateTime CreatedAt);

        /// <summary>
        /// Encode the payload into a Base64 string.
        /// </summary>
        public static string Encode(CursorPayload payload)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decode the Base64 string back into the payload object.
        /// </summary>
        public static CursorPayload? Decode(string cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return null;

            try
            {
                var bytes = Convert.FromBase64String(cursor);
                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<CursorPayload>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}
