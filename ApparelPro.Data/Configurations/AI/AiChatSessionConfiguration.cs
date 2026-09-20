using ApparelPro.Data.Models.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApparelPro.Data.Configurations.AI;

public class AiChatSessionConfiguration : IEntityTypeConfiguration<AiChatSession>
{
    public void Configure(EntityTypeBuilder<AiChatSession> builder)
    {
        builder.ToTable("AiChatSessions");

        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.SessionId)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(s => s.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.EntityKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Title)
            .HasMaxLength(200);

        builder.Property(s => s.EntityDataSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.LastMessageAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        // ─── Indexes ─────────────────────────────────────────
        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_AiChatSessions_UserId");

        builder.HasIndex(s => new { s.UserId, s.IsActive, s.LastMessageAt })
            .HasDatabaseName("IX_AiChatSessions_UserActive_LastMsg")
            .IsDescending(false, false, true);

        builder.HasIndex(s => new { s.EntityType, s.EntityKey })
            .HasDatabaseName("IX_AiChatSessions_Entity");
    }
}
