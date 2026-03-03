namespace EmployeesModule.Infrastructure.Data.Context;

using EmployeesModule.Infrastructure.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore.Extensions;

public sealed class EmployeesDbContextFactory : IDesignTimeDbContextFactory<EmployeesDbContext>
{
    public EmployeesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<EmployeesDbContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<EmployeesDbContext>();
        var connectionString = configuration.GetConnectionString("EmployeesModule")
            ?? "Server=localhost;Database=employees;User=root;Password=password";

        optionsBuilder.UseMySQL(connectionString, options =>
        {
            options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());

        return new EmployeesDbContext(optionsBuilder.Options);
    }
}
