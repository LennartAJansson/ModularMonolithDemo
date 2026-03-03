namespace EmployeesModule.Extensions;

using EmployeesModule.Infrastructure.Data.Context;
using EmployeesModule.Infrastructure.Data.Interceptors;

using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEmployeesModule(IConfiguration configuration)
        {
            services.AddEmployeesDatabase(configuration);
            services.AddEmployeesEndpoints();

            return services;
        }

        public IServiceCollection AddEmployeesDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("EmployeesModule")
                ?? throw new InvalidOperationException("EmployeesModule connection string not found");

            services.AddDbContext<EmployeesDbContext>(options =>
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

        public IServiceCollection AddEmployeesEndpoints()
        {
            // FastEndpoints registration is handled by the host application
            // Endpoints in this assembly will be discovered automatically
            services.AddEndpointsApiExplorer();

            return services;
        }
    }
}
