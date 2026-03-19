namespace Botify.API.CrossCutting.Constants
{
    public static class AppPaths
    {
        public const string AppSettingsFile = "appsettings.json";
        public const string WwwRootFolder = "wwwroot";
    }

    public static class LoggingConstants
    {
        public const string TraceId = "traceId";
    }

    public static class JwtConstants
    {
        public const string OptionsSection = "JWTOptions";
    }

    public static class CorsConstants
    {
        public const string OptionsSection = "CorsOptions";
        public const string DevPolicy = "DevCors";
        public const string ProdPolicy = "ProdCors";
    }

    public static class WebhookConstants
    {
        public const string OptionsSection = "WebhookOptions";
    }
}
