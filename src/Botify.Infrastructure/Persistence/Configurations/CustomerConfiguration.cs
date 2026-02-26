using Botify.Domain.Models.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customers");

            // Primary Key
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedNever();

            // External UUID
            builder.Property(c => c.Uuid)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(c => c.Uuid)
                .IsUnique();

            // MerchantId
            builder.Property(c => c.MerchantId)
                .IsRequired();

            // Name
            builder.Property(c => c.Name)
                .HasMaxLength(200)
                .IsRequired();

            // PhoneNumber
            builder.Property(c => c.PhoneNumber)
                .HasMaxLength(50)
                .IsRequired();

            // Email
            builder.Property(c => c.Email)
                .HasMaxLength(200);

            builder.HasIndex(c => c.Email);

            // Social Profiles (flattened)
            builder.OwnsOne(c => c.Telegram, telegram =>
            {
                telegram.Property(t => t.Link).HasMaxLength(200);
                telegram.Property(t => t.ChatId).HasMaxLength(100);
            });

            builder.OwnsOne(c => c.WhatsApp, whatsapp =>
            {
                whatsapp.Property(w => w.Link).HasMaxLength(200);
                whatsapp.Property(w => w.ChatId).HasMaxLength(100);
            });

            builder.OwnsOne(c => c.Messenger, messenger =>
            {
                messenger.Property(m => m.Link).HasMaxLength(200);
                messenger.Property(m => m.ChatId).HasMaxLength(100);
            });
            builder.HasQueryFilter(ApplicationDbContext.SOFT_DELETE_FILTER_NAME, m => !m.IsDeleted);


        }
    }
}
