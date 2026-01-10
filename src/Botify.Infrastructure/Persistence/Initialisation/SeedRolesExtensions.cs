using Botify.Domain.Identity.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedRolesExtensions
{
    public static async Task SeedRolesAsync(this ApplicationDbContext context, RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        var roles = new[] { Roles.MERCHANT.ToString(), Roles.ADMIN.ToString(), Roles.USER.ToString() };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning($"Failed to create role '{roleName}': {errors}");
                }
            }
        }
    }
}
