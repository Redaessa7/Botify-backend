using System.Text.Json;
using Botify.Application.Features.AI.Strategies;
using Botify.Domain.ResultPattern;
using Botify.Domain.Models.AI;
using Microsoft.Extensions.Caching.Distributed;
using Botify.Application.Features.AI.Requests;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class GeminiStrategy : IAIPlatformStrategy
    {
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;

        public GeminiStrategy(HttpClient client, IDistributedCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task<Result<Success>> ValidateTokenAsync(TokenValidationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ApiKey))
                return AIConfigurationErrors.ApiKeyRequired;

            if (string.IsNullOrWhiteSpace(request.Model))
                return AIConfigurationErrors.ModelNameRequired;

            var endpoint = request.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta/models";

            // 1. Validate token with lightweight request first
            var tokenResponse = await _client.GetAsync($"{endpoint}?key={request.ApiKey}");
            if (!tokenResponse.IsSuccessStatusCode)
                return AIConfigurationErrors.ApiKeyInvalid;

            // 2. Validate model (with Redis caching)
            var modelsResult = await FetchAndCacheModelsAsync(request.ApiKey, endpoint, "ai:gemini:models");
            if (modelsResult.IsError)
                return modelsResult.Errors;

            var models = modelsResult.Value;
            if (!models.Contains(request.Model))
                return AIConfigurationErrors.ModelNotFound(request.Model);

            return Result.Success;
        }

        /// <summary>
        /// Fetch models from Gemini API and cache them in Redis.
        /// Stores both full IDs (e.g. "models/gemini-1.5-pro") and shorthand ("gemini-1.5-pro").
        /// </summary>
        private async Task<Result<HashSet<string>>> FetchAndCacheModelsAsync(string apiKey, string endpoint, string cacheKey)
        {
            var cachedModels = await _cache.GetStringAsync(cacheKey);
            HashSet<string> models;
            if (!string.IsNullOrEmpty(cachedModels))
            {
                 models = new HashSet<string>(
                    cachedModels.Split(','),
                    StringComparer.OrdinalIgnoreCase
                );
                return models;
            }

            var response = await _client.GetAsync($"{endpoint}?key={apiKey}");
            if (!response.IsSuccessStatusCode)
                return AIConfigurationErrors.PlatformUnavailable;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("models", out var modelsElement))
                return AIConfigurationErrors.ModelsMissing;

             models = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var m in modelsElement.EnumerateArray())
            {
                var fullName = m.GetProperty("name").GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(fullName)) continue;

                // Add full ID (e.g. "models/gemini-1.5-pro")
                models.Add(fullName);

                // Add shorthand (strip "models/")
                var parts = fullName.Split('/');
                if (parts.Length > 1)
                    models.Add(parts[1]);
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(cacheKey, string.Join(",", models), options);

            return models;
        }
    }
}
