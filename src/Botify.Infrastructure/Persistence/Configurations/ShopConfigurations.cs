using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class ShopConfigurations : IEntityTypeConfiguration<ShopSettings>
    {
        public void Configure(EntityTypeBuilder<ShopSettings> builder)
        {

                builder.HasKey(x => x.Id);

            builder.ToTable("ShopSettings");

            builder.Property(s => s.Name)
                    .HasMaxLength(200)
                    .IsRequired();

            builder.Property(s => s.IndustryName)
                    .HasMaxLength(200);

            builder.Property(s => s.Currency)
                    .HasMaxLength(10)
                    .IsRequired();

            builder.Property(s => s.Language)
                    .HasMaxLength(50)
                    .IsRequired();

            builder.Property(s => s.Address)
                    .HasMaxLength(500)
                    .IsRequired();

            builder.Property(s => s.BusinessEmail)
                    .HasMaxLength(200);

            builder.Property(s => s.BusinessPhoneNumber)
                    .HasMaxLength(50);

            builder.Property(s => s.LogoImageUrl)
                    .HasMaxLength(500);

            builder.Property(s => s.CoverImageUrl)
                    .HasMaxLength(500);

            builder.Property(s => s.Description)
                    .HasMaxLength(1000);

            builder.Property(s => s.Timezone)
                    .HasMaxLength(100);

            builder.Property(s => s.TaxRate);

            builder.Property(s => s.DeliveryEnabled);
            builder.Property(s => s.PickupEnabled);
            builder.Property(s => s.MinimumOrder);
            builder.Property(s => s.DeliveryFee);
            builder.Property(s => s.IsOpen);

            // SocialMediaLinks as owned value object
            builder.OwnsOne(s => s.SocialMediaLinks, sm =>
                {
                    sm.Property(l => l.Whatsapp).HasMaxLength(100);
                    sm.Property(l => l.Website).HasMaxLength(200);
                    sm.Property(l => l.Facebook).HasMaxLength(200);
                    sm.Property(l => l.Twitter).HasMaxLength(200);
                    sm.Property(l => l.Instagram).HasMaxLength(200);
                    sm.Property(l => l.LinkedIn).HasMaxLength(200);
                });

            // OpeningHours as owned value object
            builder.OwnsOne(s => s.OpeningHours, oh =>
                {
                    oh.OwnsOne(o => o.Friday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); });
                    oh.OwnsOne(o => o.Saturday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); });
                    oh.OwnsOne(o => o.Sunday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); });
                    oh.OwnsOne(o => o.Monday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); }); oh.OwnsOne(o => o.Tuesday);
                    oh.OwnsOne(o => o.Tuesday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); }); oh.OwnsOne(o => o.Thursday);
                    oh.OwnsOne(o => o.Thursday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); }); oh.OwnsOne(o => o.Saturday);
                    oh.OwnsOne(o => o.Wednesday, d => { d.Property(x => x.Open); d.Property(x => x.Close); d.Property(x => x.IsOpen); });
                });
        }
    }
}
