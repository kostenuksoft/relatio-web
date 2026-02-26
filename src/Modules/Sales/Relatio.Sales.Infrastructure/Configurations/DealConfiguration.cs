using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relatio.Sales.Domain.Constants;
using Relatio.Sales.Domain.Entities;

namespace Relatio.Sales.Infrastructure.Configurations;

public sealed class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("deals");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.Title)
            .HasColumnName("title")
            .HasMaxLength(DealConstraints.TitleMaxLength)
            .IsRequired();

        builder.Property(d => d.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(d => d.Currency)
            .HasColumnName("currency")
            .HasMaxLength(DealConstraints.CurrencyMaxLength)
            .IsRequired();

        builder.Property(d => d.Stage)
            .HasColumnName("stage")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(d => d.ExpectedCloseDate)
            .HasColumnName("expected_close_date")
            .HasColumnType("timestamptz");

        builder.Property(d => d.Notes)
            .HasColumnName("notes")
            .HasMaxLength(DealConstraints.NotesMaxLength);

        builder.Property(d => d.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.HasQueryFilter(d => !d.IsDeleted);

        builder.Ignore(d => d.DomainEvents);
    }
}
