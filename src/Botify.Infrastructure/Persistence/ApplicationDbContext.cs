using Botify.Application.CrossCutting.Interfaces;
using Botify.Domain.Identity;
using Botify.Domain.Models;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Products;
using Botify.Domain.Models.Questions;
using Botify.Domain.Models.Shops;
using Botify.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Merchant> Merchants =>  Set<Merchant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AIConfiguration> AIConfigurations => Set<AIConfiguration>();
    public DbSet<Bot> Bots => Set<Bot>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ShopSettings> ShopSettings => Set<ShopSettings>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<MerchantSubscription> MerchantSubscription => Set<MerchantSubscription>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(builder);
    }
}