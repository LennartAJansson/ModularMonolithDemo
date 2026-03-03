namespace WorkloadsModule.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkloadsModule.Entities;

public sealed class WorkloadCustomerEntityTypeConfiguration : IEntityTypeConfiguration<WorkloadCustomer>
{
    public void Configure(EntityTypeBuilder<WorkloadCustomer> builder)
    {
        builder.ToTable("workload_customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        // Indexes
        builder.HasIndex(c => c.Email)
            .HasDatabaseName("ix_workload_customers_email");

        // Soft delete query filter
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
