using Botify.Domain.Models.Merchants;
using Botify.Infrastructure.Identity;
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

            // Properties
            builder.Property(m => m.FullName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(m => m.ImageUrl)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(m => m.Status)
                   .IsRequired();

            builder.Property(m => m.CreatedAt)
                   .IsRequired();

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

            // One-to-one relationship with ShopDetails
            builder.OwnsOne(m => m.ShopDetails, shop =>
            {
                shop.Property(s => s.Name).HasMaxLength(200).IsRequired();
                shop.Property(s => s.Currency).HasMaxLength(10).IsRequired();
                shop.Property(s => s.Language).HasMaxLength(50).IsRequired();
                shop.Property(s => s.Address).HasMaxLength(500).IsRequired();
                shop.Property(s => s.ImageUrl).HasMaxLength(500);
                shop.Property(s => s.TaxRate);

                shop.ToTable("ShopDetails");
            });

            // One-to-many relationship with BotConfigs
            builder.HasMany(m => m.BotConfigs)
                   .WithOne()
                   .HasForeignKey(b => b.MerchantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<Merchant>(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
