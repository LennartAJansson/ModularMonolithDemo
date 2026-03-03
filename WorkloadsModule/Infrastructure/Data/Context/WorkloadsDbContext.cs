namespace WorkloadsModule.Infrastructure.Data.Context;

using Microsoft.EntityFrameworkCore;
using WorkloadsModule.Entities;

/// <summary>
/// WorkloadsModule database context.
/// </summary>
/// <remarks>
/// To create a new migration, run:
/// <code>
/// dotnet ef migrations add [MigrationName] --project WorkloadsModule\WorkloadsModule.csproj --startup-project WorkloadsApi\WorkloadsApi.csproj --context WorkloadsDbContext --output-dir Infrastructure\Data\Migrations
/// </code>
/// To update the database, run:
/// <code>
/// dotnet ef database update --project WorkloadsModule\WorkloadsModule.csproj --startup-project WorkloadsApi\WorkloadsApi.csproj --context WorkloadsDbContext
/// </code>
/// </remarks>
public sealed class WorkloadsDbContext(DbContextOptions<WorkloadsDbContext> options) : DbContext(options)
{
    public DbSet<Workload> Workloads => Set<Workload>();
    public DbSet<WorkloadCustomer> WorkloadCustomers => Set<WorkloadCustomer>();
    public DbSet<WorkloadEmployee> WorkloadEmployees => Set<WorkloadEmployee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkloadsDbContext).Assembly);
}
