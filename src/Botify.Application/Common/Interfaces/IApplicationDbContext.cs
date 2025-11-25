using Botify.Domain.Models.Merchants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Merchant> Merchants { get; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
