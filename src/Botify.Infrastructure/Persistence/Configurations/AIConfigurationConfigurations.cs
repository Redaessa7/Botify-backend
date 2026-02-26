using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class AIConfigurationConfigurations : IEntityTypeConfiguration<AIConfiguration>
    {
        public void Configure(EntityTypeBuilder<AIConfiguration> builder)
        {
            // Table name
            builder.ToTable("AIConfigurations");

            // Primary key
            builder.HasKey(ai => ai.Id);

            // Properties
            builder.Property(ai => ai.ModelName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(ai => ai.ApiKey)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(ai => ai.Platform)
                   .HasConversion(
                       v => v.ToString(),                       
                       v => (AIPlatform)Enum.Parse(typeof(AIPlatform), v)
                   )
                   .IsRequired();

            builder.Property(ai => ai.Temperature)
                   .HasDefaultValue(0.7);

            builder.Property(ai => ai.MaxTokens)
                   .IsRequired();

            builder.Property(ai => ai.MerchantId)
                   .IsRequired();

                    builder.Property(ai => ai.OrderProcessingEnabled)
               .IsRequired()
               .HasDefaultValue(true);

            builder.Property(ai => ai.ProductRecommendationsEnabled)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(ai => ai.SmallTalkEnabled)
                   .IsRequired()
                   .HasDefaultValue(true);

            builder.Property(ai => ai.AutoEscalateEnabled)
                   .IsRequired()
                   .HasDefaultValue(true);
            // Relationships
            builder.HasOne<Merchant>() 
                   .WithOne(x => x.AIConfiguration) 
                   .HasForeignKey<AIConfiguration>(ai => ai.MerchantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(ApplicationDbContext.SOFT_DELETE_FILTER_NAME, m => !m.IsDeleted);


        }
    }
}
