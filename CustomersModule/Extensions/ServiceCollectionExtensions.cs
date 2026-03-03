namespace CustomersModule.Extensions;

using CustomersModule.Infrastructure.Data.Context;
using CustomersModule.Infrastructure.Data.Interceptors;

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
        public IServiceCollection AddCustomersModule(IConfiguration configuration)
        {
            services.AddCustomersDatabase(configuration);
            services.AddCustomersEndpoints();

            return services;
        }

        public IServiceCollection AddCustomersDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CustomersModule")
                ?? throw new InvalidOperationException("CustomersModule connection string not found");

            services.AddDbContext<CustomersDbContext>(options =>
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

        public IServiceCollection AddCustomersEndpoints()
        {
            // FastEndpoints registration is handled by the host application
            // Endpoints in this assembly will be discovered automatically
            services.AddEndpointsApiExplorer();

            return services;
        }
    }
}
