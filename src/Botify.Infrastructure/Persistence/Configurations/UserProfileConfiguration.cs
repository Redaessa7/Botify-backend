using Botify.Domain.Identity;
using Botify.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class UserProfileConfigurations : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("UserProfiles");

            // Primary Key
            builder.HasKey(b => b.Id);

            // Properties
            builder.Property(x => x.IdentityUserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(x => x.FirstName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.LastName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Bio)
                   .HasMaxLength(1000);

            builder.Property(x => x.ProfileImageUrl)
                   .HasMaxLength(500);

            builder.Property(x => x.Language)
                   .HasMaxLength(50);

            builder.Property(x => x.TimeZone)
                   .HasMaxLength(100);

            builder.HasIndex(x => x.IdentityUserId).IsUnique();

            // One-to-One relationship
            builder.HasOne<ApplicationUser>()
                   .WithOne()
                   .HasForeignKey<UserProfile>(x => x.IdentityUserId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();
        }
    }
}
