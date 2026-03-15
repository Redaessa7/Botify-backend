using Botify.Domain.Models.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();

            // UUID string property
            builder.Property(o => o.Uuid)
                .HasMaxLength(255) // adjust length depending on your UUID format
                .IsRequired();

            builder.HasIndex(o => o.Uuid)
                .IsUnique(); // enforce uniqueness at DB level

            builder.Property(o => o.MerchantId)
                .IsRequired();

            builder.Property(o => o.CustomerId)
                .IsRequired();

            builder.Property(o => o.Platform)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(o => o.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.Currency)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(o => o.DeliveryAddress)
                .HasMaxLength(500);

            builder.Property(o => o.Notes)
                .HasMaxLength(1000);

            builder.HasOne(o => o.Customer)
                   .WithMany()
                   .HasForeignKey(o => o.CustomerId);

            // Configure owned collection of OrderItems
            builder.OwnsMany(o => o.Items, items =>
            {
                items.ToTable("OrderItems");

                items.WithOwner().HasForeignKey(i => i.OrderId);

                items.HasKey(i => new { i.OrderId, i.ProductId });
                items.Property(i => i.ProductId)
                    .IsRequired();

                items.Property(i => i.ProductName)
                    .HasMaxLength(200)
                    .IsRequired();

                items.Property(i => i.Quantity)
                    .IsRequired();

                items.Property(i => i.UnitPrice)
                    .HasPrecision(18, 2)
                    .IsRequired();
            });

            builder.Navigation(o => o.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            builder.HasQueryFilter(ApplicationDbContext.SOFT_DELETE_FILTER_NAME, m => !m.IsDeleted);

        }
    }
}
