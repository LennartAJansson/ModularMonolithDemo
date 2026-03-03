namespace WorkloadsModule.Infrastructure.Data.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkloadsModule.Entities;

public sealed class WorkloadEntityTypeConfiguration : IEntityTypeConfiguration<Workload>
{
    public void Configure(EntityTypeBuilder<Workload> builder)
    {
        builder.ToTable("workloads");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(w => w.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(w => w.StopDate)
            .HasColumnName("stop_date");

        builder.Property(w => w.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1000);

        builder.Property(w => w.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(w => w.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(w => w.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        // Relationships
        builder.HasOne(w => w.Customer)
            .WithMany(c => c.Workloads)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Employee)
            .WithMany(e => e.Workloads)
            .HasForeignKey(w => w.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(w => w.StartDate)
            .HasDatabaseName("ix_workloads_start_date");

        builder.HasIndex(w => w.CustomerId)
            .HasDatabaseName("ix_workloads_customer_id");

        builder.HasIndex(w => w.EmployeeId)
            .HasDatabaseName("ix_workloads_employee_id");

        // Soft delete query filter
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}
