using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relatio.Customers.Domain.Constants;
using Relatio.Customers.Domain.Entities;
using Relatio.Shared.ValueObjects;

namespace Relatio.Customers.Infrastructure.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(CustomerConstraints.NameMaxLength)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(CustomerConstraints.EmailMaxLength)
            .IsRequired()
            .HasConversion(
                email => email.Value,
                value => Email.CreateUnsafe(value));

        builder.HasIndex(c => c.Email)
            .IsUnique();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(CustomerConstraints.PhoneMaxLength)
            .HasConversion(
                phone => phone == null ? null : phone.Value,
                value => value == null ? null : PhoneNumber.CreateUnsafe(value));

        builder.Property(c => c.Industry)
            .HasColumnName("industry")
            .HasMaxLength(CustomerConstraints.IndustryMaxLength);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasMaxLength(CustomerConstraints.StatusMaxLength)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Ignore(c => c.DomainEvents);
    }
}
