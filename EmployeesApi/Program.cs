using EmployeesApi.Infrastructure.Observability;
using EmployeesModule.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;
using MicroEvents;
using Scalar.AspNetCore;
using Serilog;

try
{
    Log.Information("Starting EmployeesApi");

    var builder = WebApplication.CreateBuilder(args);

    // Add Observability (Serilog, OpenTelemetry, HealthChecks)
    builder.AddEmployeesApiObservability();

    // Add MicroEvents (NATS-based event bus)
    builder.Services.AddMicroEvents(builder.Configuration);

    // Add FastEndpoints with EmployeesModule assembly
    builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(EmployeesModule.Extensions.ServiceCollectionExtensions).Assembly])
        .SwaggerDocument(s => s.ShortSchemaNames = true);

    // Add EmployeesModule
    builder.Services.AddEmployeesModule(builder.Configuration);

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Migrate database
    app.MigrateEmployeesDatabase();

    app.UseExceptionHandler();
    app.UseHttpsRedirection();

    // Use observability (health checks, metrics, request logging)
    app.UseEmployeesApiObservability();

    // Use FastEndpoints
    app.UseFastEndpoints(c =>
    {
        c.Errors.ResponseBuilder = (failures, ctx, statusCode) => new ErrorResponse
        {
            StatusCode = statusCode,
            Message = "One or more errors occurred!",
            Errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToList())
        };
    });

    _ = app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");
    _ = app.MapScalarApiReference();

    Log.Information("EmployeesApi is ready");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "EmployeesApi terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
