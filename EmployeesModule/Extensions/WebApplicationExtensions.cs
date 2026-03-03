namespace EmployeesModule.Extensions;

using EmployeesModule.Infrastructure.Data.Context;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication UseEmployeesModule()
        {
            // FastEndpoints middleware is configured by the host application
            // This module only configures OpenAPI and Scalar
            //_ = app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
            //_ = app.MapScalarApiReference();

            return app;
        }

        public WebApplication MigrateEmployeesDatabase()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmployeesDbContext>();
            dbContext.Database.Migrate();
            return app;
        }
    }
}
