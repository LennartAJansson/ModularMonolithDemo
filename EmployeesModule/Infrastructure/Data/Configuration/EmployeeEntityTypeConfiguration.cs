namespace EmployeesModule.Infrastructure.Data.Configuration;

using EmployeesModule.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class EmployeeEntityTypeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.SSN)
            .HasColumnName("ssn")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Address)
            .HasColumnName("address")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(e => e.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Salary)
            .HasColumnName("salary")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(e => e.HireDate)
            .HasColumnName("hire_date")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        // Unique indexes
        builder.HasIndex(e => e.SSN)
            .IsUnique()
            .HasDatabaseName("ix_employees_ssn");

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("ix_employees_email");

        // Soft delete query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
