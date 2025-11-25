using Botify.Domain.Models.Merchants.BotConfigs;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class BotConfigConfigurations : IEntityTypeConfiguration<BotConfig>
    {
        public void Configure(EntityTypeBuilder<BotConfig> builder)
        {
            builder.ToTable("BotConfigs");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Token).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Secret).IsRequired().HasMaxLength(200);
            builder.Property(b => b.WebhookUrl).HasMaxLength(500);

            builder.Property(b => b.Enabled).IsRequired();
            builder.Property(b => b.PlatformVerified).IsRequired();
            builder.Property(b => b.IsPrimary).IsRequired();
            builder.Property(b => b.TargetPlatform).HasConversion(o => o.Name, c => new BotPlatform(c));
        }
    }

}
