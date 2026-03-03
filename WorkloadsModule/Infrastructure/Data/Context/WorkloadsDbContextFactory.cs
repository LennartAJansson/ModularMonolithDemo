namespace WorkloadsModule.Infrastructure.Data.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MySql.EntityFrameworkCore.Extensions;
using WorkloadsModule.Infrastructure.Data.Interceptors;

public sealed class WorkloadsDbContextFactory : IDesignTimeDbContextFactory<WorkloadsDbContext>
{
    public WorkloadsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<WorkloadsDbContextFactory>()
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<WorkloadsDbContext>();
        var connectionString = configuration.GetConnectionString("WorkloadsModule")
            ?? "Server=localhost;Database=workloads;User=root;Password=password";

        optionsBuilder.UseMySQL(connectionString, options =>
        {
            options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        optionsBuilder.AddInterceptors(new AuditableEntityInterceptor());

        return new WorkloadsDbContext(optionsBuilder.Options);
    }
}
