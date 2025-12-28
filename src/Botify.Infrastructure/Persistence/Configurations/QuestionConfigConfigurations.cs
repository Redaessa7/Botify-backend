using Botify.Domain.Models.Merchants.Questions;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class QuestionConfigConfigurations : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.ToTable("Questions");
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
                    c => Enum.Parse<QuestionType>(c, true)
                )
                .IsRequired();

            builder.Property(q => q.ResolvedAt);

            // ✅ Configure CustomerInfo as an owned entity
            builder.OwnsOne(q => q.Customer, customer =>
            {
                customer.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                customer.Property(c => c.Email)
                    .HasMaxLength(200);

                customer.Property(c => c.Telegram)
                    .HasMaxLength(100);

                customer.Property(c => c.Whatsapp)
                    .HasMaxLength(100);

                customer.Property(c => c.Messenger)
                    .HasMaxLength(100);
            });
        }
    }
}
