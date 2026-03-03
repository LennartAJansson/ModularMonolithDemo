namespace WorkloadsModule.Extensions;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;
using WorkloadsModule.Infrastructure.Data.Context;
using WorkloadsModule.Infrastructure.Data.Interceptors;
using WorkloadsModule.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWorkloadsModule(IConfiguration configuration)
        {
            services.AddWorkloadsDatabase(configuration);
            services.AddWorkloadsEndpoints();
            services.AddWorkloadsEventSubscribers();

            return services;
        }

        public IServiceCollection AddWorkloadsDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("WorkloadsModule")
                ?? throw new InvalidOperationException("WorkloadsModule connection string not found");

            services.AddDbContext<WorkloadsDbContext>(options =>
            {
                options.UseMySQL(connectionString, mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);

                    mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

                options.AddInterceptors(new AuditableEntityInterceptor());
            });

            return services;
        }

        public IServiceCollection AddWorkloadsEndpoints()
        {
            // FastEndpoints registration is handled by the host application
            // Endpoints in this assembly will be discovered automatically
            services.AddEndpointsApiExplorer();

            return services;
        }

        public IServiceCollection AddWorkloadsEventSubscribers()
        {
            services.AddHostedService<CustomerEventsSubscriber>();
            services.AddHostedService<EmployeeEventsSubscriber>();

            return services;
        }
    }
}
