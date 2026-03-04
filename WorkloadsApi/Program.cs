using FastEndpoints;
using FastEndpoints.Swagger;
using MicroEvents;
using Scalar.AspNetCore;
using Serilog;
using WorkloadsApi.Infrastructure.Observability;
using WorkloadsModule.Extensions;

try
{
    Log.Information("Starting WorkloadsApi");

    var builder = WebApplication.CreateBuilder(args);

    // Add Observability (Serilog, OpenTelemetry, HealthChecks)
    builder.AddWorkloadsApiObservability();

    // Add MicroEvents (NATS-based event bus)
    builder.Services.AddMicroEvents(builder.Configuration);

    // Add FastEndpoints with WorkloadsModule assembly
    builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(WorkloadsModule.Extensions.ServiceCollectionExtensions).Assembly])
        .SwaggerDocument(s => s.ShortSchemaNames = true);

    // Add WorkloadsModule
    builder.Services.AddWorkloadsModule(builder.Configuration);

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Migrate database
    app.MigrateWorkloadsDatabase();

    app.UseExceptionHandler();
    app.UseHttpsRedirection();

    // Use observability (health checks, metrics, request logging)
    app.UseWorkloadsApiObservability();

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

    Log.Information("WorkloadsApi is ready");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "WorkloadsApi terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
