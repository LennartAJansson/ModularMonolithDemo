namespace WorkloadsModule.Extensions;

using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using WorkloadsModule.Infrastructure.Data.Context;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication UseWorkloadsModule()
        {
            // FastEndpoints middleware is configured by the host application
            // This module only configures OpenAPI and Scalar
            //_ = app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
            //_ = app.MapScalarApiReference();

            return app;
        }

        public WebApplication MigrateWorkloadsDatabase()
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WorkloadsDbContext>();
            dbContext.Database.Migrate();
            return app;
        }
    }
}
