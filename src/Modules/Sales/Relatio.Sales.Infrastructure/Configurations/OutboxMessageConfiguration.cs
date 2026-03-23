using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relatio.Sales.Infrastructure.Outbox;

namespace Relatio.Sales.Infrastructure.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(m => m.Payload)
            .HasColumnName("payload")
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .HasColumnName("processed_at")
            .HasColumnType("timestamptz");

        builder.Property(m => m.Error)
            .HasColumnName("error")
            .HasMaxLength(2048);

        builder.Property(m => m.RetryCount)
            .HasColumnName("retry_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(m => m.CreatedAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("ix_outbox_messages_unprocessed");
    }
}
