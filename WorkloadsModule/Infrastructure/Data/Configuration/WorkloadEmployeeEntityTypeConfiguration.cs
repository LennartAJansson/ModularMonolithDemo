namespace WorkloadsModule.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkloadsModule.Entities;

public sealed class WorkloadEmployeeEntityTypeConfiguration : IEntityTypeConfiguration<WorkloadEmployee>
{
    public void Configure(EntityTypeBuilder<WorkloadEmployee> builder)
    {
        builder.ToTable("workload_employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
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

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => e.Email)
            .HasDatabaseName("ix_workload_employees_email");

        // Soft delete query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
