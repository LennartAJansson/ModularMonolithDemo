namespace EmployeesModule.Infrastructure.Data.Context;

using EmployeesModule.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// EmployeesModule database context.
/// </summary>
/// <remarks>
/// To create a new migration, run:
/// <code>
/// dotnet ef migrations add [MigrationName] --project EmployeesModule\EmployeesModule.csproj --startup-project EmployeesApi\EmployeesApi.csproj --context EmployeesDbContext --output-dir Infrastructure\Data\Migrations
/// </code>
/// To update the database, run:
/// <code>
/// dotnet ef database update --project EmployeesModule\EmployeesModule.csproj --startup-project EmployeesApi\EmployeesApi.csproj --context EmployeesDbContext
/// </code>
/// </remarks>
public sealed class EmployeesDbContext(DbContextOptions<EmployeesDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeesDbContext).Assembly);
}
