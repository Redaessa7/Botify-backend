using Botify.Domain.Models.Bots;
using Botify.Domain.Models.Bots.ValueObjects;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    public sealed class BotConfigConfigurations : IEntityTypeConfiguration<Bot>
    {
        public void Configure(EntityTypeBuilder<Bot> builder)
        {
            builder.ToTable("Bots");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedNever();

            builder.Property(b => b.Token)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Secret)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.WebhookUrl)
                .HasMaxLength(500);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(b => b.Active).IsRequired();
            builder.Property(b => b.PlatformVerified).IsRequired();
            builder.Property(b => b.IsPrimary).IsRequired();

            builder.Property(b => b.Platform)
                .HasConversion(
                    o => o.ToString().ToUpperInvariant(),
                    c => Enum.Parse<BotPlatform>(c, true)
                );

            builder.HasMany(x => x.Questions)
                .WithOne()
                .HasForeignKey(q => q.BotId)
                .OnDelete(DeleteBehavior.Cascade);

            // 👇 Owned collection for BotDailyStats (like OrderItems)
            builder.OwnsMany(b => b.DailyStats, stats =>
            {
                stats.ToTable("BotDailyStats");

                stats.WithOwner().HasForeignKey(x => x.BotId);

                stats.HasKey(x => new { x.BotId, x.Date }); // composite key: one record per bot per day

                stats.Property(s => s.Date)
                    .IsRequired();

                stats.Property(s => s.MessageCount)
                    .IsRequired();

                stats.Property(s => s.OrderCount)
                    .IsRequired();
            });

            builder.Property(b => b.LastPing) 
                .IsRequired(false); 


            builder.Navigation(b => b.DailyStats)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
