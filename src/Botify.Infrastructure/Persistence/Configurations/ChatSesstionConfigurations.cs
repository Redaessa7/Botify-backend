using Botify.Domain.Models.AI;
using Botify.Domain.Models.Merchants;
using Botify.Infrastructure.AiAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Botify.Infrastructure.Persistence.Configurations
{
    internal class ChatSessionConfigurations : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.ToTable("ChatSessions");

            // 1. Primary Key
            builder.HasKey(x => x.Id);

            // 2. Composite Index for Performance
            // We search by ExternalChatId and Platform on every message. 
            // An index here makes the 'FirstOrDefaultAsync' lightning fast.
            builder.HasIndex(x => new { x.ExternalChatId, x.Platform })
                   .IsUnique();

            // 3. Properties
            builder.Property(x => x.ExternalChatId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Platform)
                   .IsRequired()
                   .HasConversion<string>(); // Stores 'Telegram' instead of '0' for readability

            builder.Property(x => x.CurrentSummary)
                   .IsRequired(false);
 
            builder.Property(x => x.MessageCount)
                   .HasDefaultValue(0);

 
        }
    }
}