using Botify.Domain.Models.Merchants;
using Botify.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(a => a.FirstName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.LastName)
            .IsRequired()
            .HasMaxLength(255);
    }
}
