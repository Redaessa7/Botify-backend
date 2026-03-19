using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class QuestionConfigConfigurations : IEntityTypeConfiguration<SupportRequest>
    {
        public void Configure(EntityTypeBuilder<SupportRequest> builder)
        {
            builder.ToTable("SupportRequests");
            builder.HasKey(q => q.Id);

            builder.Property(q => q.BotName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(q => q.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(q => q.IsResolved)
                .IsRequired();

            builder.Property(q => q.Platform)
                .HasConversion(
                    o => o.ToString().ToUpperInvariant(),
                    c => Enum.Parse<BotPlatform>(c, true)
                )
                .IsRequired();

            builder.Property(q => q.Type)
                .HasConversion(
                    o => o.ToString().ToUpperInvariant(),
                    c => Enum.Parse<SupportRequestType>(c, true)
                )
                .IsRequired();

            builder.Property(q => q.ResolvedAt);

         builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(ApplicationDbContext.SOFT_DELETE_FILTER_NAME, m => !m.IsDeleted);

        }
    }
}
