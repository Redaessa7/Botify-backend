using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Botify.Application.Features.AI.Strategies;
using Botify.Domain.ResultPattern;
using Botify.Domain.Models.AI;
using Microsoft.Extensions.Caching.Distributed;
using Botify.Application.Features.AI.Requests;

namespace Botify.Infrastructure.AIServices.Strategies
{
    public sealed class GitHubInferenceStrategy : IAIPlatformStrategy
    {
        private readonly HttpClient _client;
        private readonly IDistributedCache _cache;

        public GitHubInferenceStrategy(HttpClient client, IDistributedCache cache)
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

            try
            {
                // 1. Validate model first (with Redis caching)
                var modelsResult = await FetchAndCacheModelsAsync(request.ApiKey, request.BaseUrl, "ai:github:models");
                if (modelsResult.IsError)
                    return modelsResult.Errors;

                var models = modelsResult.Value;
                if (!models.Contains(request.Model))
                    return AIConfigurationErrors.ModelNotFound(request.Model);

                // 2. Validate token with lightweight request
                var endpoint = request.BaseUrl ?? "https://models.github.ai/inference/chat/completions";

                var payload = new
                {
                    model = request.Model,
                    messages = new[]
                    {
                        new { role = "system", content = "ping" }
                    },
                    max_tokens = 1
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.ApiKey);
                _client.DefaultRequestHeaders.UserAgent.ParseAdd("BotifyApp/1.0");

                var response = await _client.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => AIConfigurationErrors.ApiKeyInvalid,
                        System.Net.HttpStatusCode.NotFound => AIConfigurationErrors.ModelNotFound(request.Model),
                        System.Net.HttpStatusCode.BadRequest => AIConfigurationErrors.EndpointInvalid,
                        _ => AIConfigurationErrors.UnknownError
                    };
                }

                return Result.Success;
            }
            catch (Exception)
            {
                return AIConfigurationErrors.PlatformUnavailable;
            }
        }

        /// <summary>
        /// Shared helper to fetch models from GitHub API and cache them in Redis.
        /// </summary>
        private async Task<Result<HashSet<string>>> FetchAndCacheModelsAsync(string apiKey, string? baseUrl, string cacheKey)
        {
            // Try to get cached models from Redis
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

            var endpoint = baseUrl ?? "https://models.github.ai/catalog/models";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            _client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            var response = await _client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                return AIConfigurationErrors.PlatformUnavailable;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return AIConfigurationErrors.ModelsMissing;

            // Extract canonical IDs like "openai/gpt-4.1" and strip prefix
             models = new HashSet<string>(
                doc.RootElement.EnumerateArray()
                    .Select(m =>
                    {
                        var id = m.GetProperty("id").GetString() ?? string.Empty;
                        // Strip publisher prefix (e.g., "openai/")
                        var parts = id.Split('/');
                        return parts.Length > 1 ? parts[1] : id;
                    }),
                StringComparer.OrdinalIgnoreCase
            );

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            await _cache.SetStringAsync(cacheKey, string.Join(",", models), options);

            return models;
        }

    }
}
