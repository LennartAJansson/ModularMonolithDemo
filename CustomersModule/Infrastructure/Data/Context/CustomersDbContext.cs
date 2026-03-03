namespace CustomersModule.Infrastructure.Data.Context;

using CustomersModule.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// CustomersModule database context.
/// </summary>
/// <remarks>
/// To create a new migration, run:
/// <code>
/// dotnet ef migrations add [MigrationName] --project CustomersModule\CustomersModule.csproj --startup-project CustomersApi\CustomersApi.csproj --context CustomersDbContext --output-dir Infrastructure\Data\Migrations
/// </code>
/// To update the database, run:
/// <code>
/// dotnet ef database update --project CustomersModule\CustomersModule.csproj --startup-project CustomersApi\CustomersApi.csproj --context CustomersDbContext
/// </code>
/// </remarks>
public sealed class CustomersDbContext(DbContextOptions<CustomersDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersDbContext).Assembly);
}
