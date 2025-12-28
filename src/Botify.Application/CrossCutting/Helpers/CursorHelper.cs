using System;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Botify.Application.CrossCutting.Helpers
{
    public static class CursorHelper
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // This is the object that holds both Id and CreatedAt
        public record CursorPayload(Guid Id, DateTimeOffset CreatedAt);

        // Encode the object into a Base64 string (optionally HMAC-signed)
        public static string Encode(CursorPayload payload, string? hmacSecret = null)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);

            if (string.IsNullOrEmpty(hmacSecret)) return base64;

            var signature = ComputeHmac(base64, hmacSecret);
            return $"{base64}.{signature}";
        }

        // Decode the string back into the object
        public static CursorPayload? Decode(string cursor, string? hmacSecret = null)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return null;

            string base64;
            if (!string.IsNullOrEmpty(hmacSecret))
            {
                var parts = cursor.Split('.', 2);
                if (parts.Length != 2) return null;

                base64 = parts[0];
                var signature = parts[1];
                var expected = ComputeHmac(base64, hmacSecret);

                if (!CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(signature),
                        Encoding.UTF8.GetBytes(expected)))
                {
                    return null;
                }
            }
            else
            {
                base64 = cursor;
            }

            try
            {
                var bytes = Convert.FromBase64String(base64);
                var json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<CursorPayload>(json, JsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static string ComputeHmac(string data, string secret)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(key);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToBase64String(hash);
        }
    }
}
