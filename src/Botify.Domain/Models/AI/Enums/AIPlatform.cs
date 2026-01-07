namespace Botify.Domain.Models.AI.Enums
{
    public enum AIPlatform
    {
        OpenAI,
        AzureOpenAI,
        Anthropic,
        GooglePaLM,
        HuggingFace,
        GitHub
    }

    public static class AIPlatformExtensions
    {
        /// <summary>
        /// Converts the enum value to a consistent uppercase string key.
        /// </summary>
        public static string AsKey(this AIPlatform platform)
            => platform.ToString().ToUpperInvariant();
    }
}
