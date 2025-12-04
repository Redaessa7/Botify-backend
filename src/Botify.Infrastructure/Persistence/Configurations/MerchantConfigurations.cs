using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Bots;
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

            builder.Property(m => m.LogoUrl)
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(m => m.Status)
                   .IsRequired();

            builder.Property(m => m.MerchantName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(m => m.Industry)
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
            builder.OwnsOne(m => m.ShopSettings, shop =>
            {
                shop.Property(s => s.Name).HasMaxLength(200).IsRequired();
                shop.Property(s => s.Currency).HasMaxLength(10).IsRequired();
                shop.Property(s => s.Language).HasMaxLength(50).IsRequired();
                shop.Property(s => s.Address).HasMaxLength(500).IsRequired();
                shop.Property(s => s.ImageUrl).HasMaxLength(500);
                shop.Property(s => s.TaxRate);

                shop.ToTable("ShopSettings");
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
        }
    }
}
