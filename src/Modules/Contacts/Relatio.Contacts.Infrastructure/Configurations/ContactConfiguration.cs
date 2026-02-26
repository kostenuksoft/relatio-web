using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relatio.Contacts.Domain.Constants;
using Relatio.Contacts.Domain.Entities;
using Relatio.Shared.ValueObjects;

namespace Relatio.Contacts.Infrastructure.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(ContactConstraints.FirstNameMaxLength)
            .IsRequired();

        builder.Property(c => c.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(ContactConstraints.LastNameMaxLength)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(ContactConstraints.EmailMaxLength)
            .HasConversion(
                email => email == null ? null : email.Value,
                value => value == null ? null : Email.CreateUnsafe(value));

        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(ContactConstraints.PhoneMaxLength)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => value == null ? null : PhoneNumber.CreateUnsafe(value));

        builder.Property(c => c.Position)
            .HasColumnName("position")
            .HasMaxLength(ContactConstraints.PositionMaxLength);

        builder.Property(c => c.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.Ignore(c => c.DomainEvents);
    }
}
