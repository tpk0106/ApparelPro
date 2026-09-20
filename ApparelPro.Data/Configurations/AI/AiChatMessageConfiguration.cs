using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI;

public class AiChatMessageConfiguration : IEntityTypeConfiguration<AiChatMessage>
{
    public void Configure(EntityTypeBuilder<AiChatMessage> builder)
    {
        builder.ToTable("AiChatMessages");

        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.MessageId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.TokensUsed)
            .HasDefaultValue(0);

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // ─── Relationships ───────────────────────────────────
        builder.HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Indexes ─────────────────────────────────────────
        builder.HasIndex(m => new { m.SessionId, m.CreatedAt })
            .HasDatabaseName("IX_AiChatMessages_Session_Created");
    }
}
