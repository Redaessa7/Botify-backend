using Botify.Domain.Identity;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.Models.Merchants.Questions;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.CrossCutting.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Merchant> Merchants { get; }
        public DbSet<UserProfile> UserProfiles { get; }
        public DbSet<AIConfiguration> AIConfigurations { get; }
        public DbSet<Question> Questions { get; }
        public DbSet<Bot> Bots { get; } 
        public DbSet<Product> Products { get; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
