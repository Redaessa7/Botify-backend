using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation;

public class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger,
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger = logger;
    private readonly ApplicationDbContext _context = context;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task InitialiseAsync()
    {
        try
        {
            // Ensure SQLite directory exists
            var connectionString = _context.Database.GetDbConnection().ConnectionString;
            var builder = new SqliteConnectionStringBuilder(connectionString);

            var dataSource = builder.DataSource;
            var directory = Path.GetDirectoryName(dataSource);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Apply migrations if needed
            if ((await _context.Database.GetPendingMigrationsAsync()).Any())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            // Roles
            await _context.SeedRolesAsync(_roleManager, _logger);

            // Merchants (with bots, questions, products, customers, orders)
            await _context.SeedMerchantsAsync(_userManager, _logger);

            // Save changes once all seeding is done
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
