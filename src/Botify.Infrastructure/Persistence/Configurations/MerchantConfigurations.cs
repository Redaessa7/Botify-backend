using Botify.Domain.Models.Merchants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class MerchantConfigurations : IEntityTypeConfiguration<Merchant>
    {
        public void Configure(EntityTypeBuilder<Merchant> builder)
        {
            // Table name
            builder.ToTable("Merchants");

            // Primary key
            builder.HasKey(m => m.Id);


            builder.Property(m => m.VerifiedAt);

            // One-to-one relationship with Subscription
            builder.OwnsOne(m => m.Subscription, subscription =>
            {
                subscription.Property(s => s.CurrentPlan)
                            .IsRequired();

                subscription.Property(s => s.ExpiryDate)
                            .IsRequired();

                subscription.ToTable("MerchantSubscriptions");
            });

            // One-to-many relationship with Bots using private backing field
            builder.HasMany(m => m.Bots)
                   .WithOne()
                   .HasForeignKey(b => b.MerchantId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Tell EF to use the private field "_bots" for change tracking
            builder.Navigation(m => m.Bots)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasOne(m => m.Profile)
                    .WithOne()
                    .HasForeignKey<Merchant>(m => m.UserProfileId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

            builder.HasMany(x => x.Products)
                     .WithOne()
                     .HasForeignKey(p => p.MerchantId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(m => m.Products)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Questions)
                     .WithOne()
                     .HasForeignKey(q => q.MerchantId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Orders)
                     .WithOne()
                     .HasForeignKey(q => q.MerchantId)
                     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
