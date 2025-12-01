using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Merchant> Merchants { get; }
        public DbSet<UserProfile> UserProfiles { get; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
