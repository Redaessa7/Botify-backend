using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Botify.Infrastructure.Persistence.Initialisation
{
    public static class InitialiserExtensions
    {
        public static async Task InitialiseDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

            await initialiser.InitialiseAsync();
            await initialiser.SeedAsync();               // Seed merchants and roles
        }
    }
}
