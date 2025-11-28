using Botify.Application.Common.Interfaces;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.BotConfigs;
using Botify.Domain.Models.Merchants.ShopsDetails;
using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{

    public DbSet<Merchant> Merchants =>  Set<Merchant>();
    public DbSet<BotConfig> BotConfigs => Set<BotConfig>();
    public DbSet<ShopDetails> ShopDetails => Set<ShopDetails>();
    public DbSet<MerchantSubscription> MerchantSubscription => Set<MerchantSubscription>();


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}