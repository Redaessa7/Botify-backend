namespace Botify.API.CrossCutting.Setup
{
    /// <summary>
    /// Provides extension methods to register all the application's services and infrastructure services 
    /// into the <see cref="IServiceCollection"/> in a centralized way.
    /// <para>
    /// This class is intended to be called from <c>Program.cs</c> to keep the startup configuration clean and organized.
    /// It aggregates service registrations from multiple layers:
    /// </para>
    /// <list type="bullet">
    ///   <item>
    ///     <description><b>Application Layer Services:</b> All business logic services, MediatR handlers, validation, mappings, and cross-cutting concerns.</description>
    ///   </item>
    ///   <item>
    ///     <description><b>Infrastructure Layer Services:</b> Repositories, persistence (EF DbContext), identity, JWT/token services, external integrations like file storage or email providers.</description>
    ///   </item>
    /// </list>
    /// <para>
    /// Example usage in Program.cs:
    /// <code>
    /// builder.Services.AddApplicationSetup(builder.Configuration, builder.Environment);
    /// </code>
    /// </para>
    /// </summary>
    public static class ApplicationSetup
    {
        /// <summary>
        /// Registers all Application and Infrastructure layer services into the DI container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <param name="env">The current hosting environment.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddApplicationSetup(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            return services
                    .AddPresentationServices(configuration, env)
                    .AddApplicationServices(configuration)
                    .AddInfrastructureServices(configuration);
        }

    }
}
