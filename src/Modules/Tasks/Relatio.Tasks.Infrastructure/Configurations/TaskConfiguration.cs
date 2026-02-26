using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relatio.Tasks.Domain.Constants;
using Relatio.Tasks.Domain.Entities;

namespace Relatio.Tasks.Infrastructure.Configurations;

public sealed class TaskConfiguration : IEntityTypeConfiguration<CrmTask>
{
    public void Configure(EntityTypeBuilder<CrmTask> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(TaskConstraints.TitleMaxLength)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(TaskConstraints.DescriptionMaxLength);

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date")
            .HasColumnType("timestamptz");

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.AssignedToUserId)
            .HasColumnName("assigned_to_user_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Ignore(t => t.DomainEvents);
    }
}
