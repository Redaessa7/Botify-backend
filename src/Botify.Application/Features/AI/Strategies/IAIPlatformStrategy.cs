namespace Botify.Application.Features.AI.Strategies
{
    public interface IAIPlatformStrategy
    {
        Task<bool> ValidateTokenAsync(string apiKey, string? baseUrl = null);
    }
}
