namespace CustomersModule.Infrastructure.Data.Context;

using CustomersModule.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore.Extensions;

public sealed class CustomersDbContextFactory : IDesignTimeDbContextFactory<CustomersDbContext>
{
    public CustomersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<CustomersDbContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<CustomersDbContext>();
        var connectionString = configuration.GetConnectionString("CustomersModule")
            ?? "Server=localhost;Database=customers;User=root;Password=password";

        optionsBuilder.UseMySQL(connectionString, options =>
        {
            options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());

        return new CustomersDbContext(optionsBuilder.Options);
    }
}
