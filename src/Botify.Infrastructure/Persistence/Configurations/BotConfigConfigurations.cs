using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class BotConfigConfigurations : IEntityTypeConfiguration<Bot>
    {
        public void Configure(EntityTypeBuilder<Bot> builder)
        {
            builder.ToTable("Bots");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Token).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Secret).IsRequired().HasMaxLength(200);
            builder.Property(b => b.WebhookUrl).HasMaxLength(500);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(500);
            builder.Property(b => b.Active).IsRequired();
            builder.Property(b => b.PlatformVerified).IsRequired();
            builder.Property(b => b.IsPrimary).IsRequired();
            builder.Property(b => b.Platform)
                .HasConversion(
                    o => o.ToString().ToUpperInvariant(),          
                    c => Enum.Parse<BotPlatform>(c, true)          
                );
            builder.Property(q => q.CreatedAtUtc)
        .HasConversion(
                v => v.UtcDateTime,
                v => new DateTimeOffset(v));

            builder.HasMany(x => x.Questions)
                .WithOne()
                .HasForeignKey(q => q.BotId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
