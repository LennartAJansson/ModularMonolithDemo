using CustomersApi.Infrastructure.Observability;
using CustomersModule.Extensions;
using FastEndpoints;
using FastEndpoints.Swagger;
using MicroEvents;
using Scalar.AspNetCore;
using Serilog;

try
{
    Log.Information("Starting CustomersApi");

    var builder = WebApplication.CreateBuilder(args);

    // Add Observability (Serilog, OpenTelemetry, HealthChecks)
    builder.AddCustomersApiObservability();

    // Add MicroEvents (NATS-based event bus)
    builder.Services.AddMicroEvents(builder.Configuration);

    // Add FastEndpoints with CustomersModule assembly
    builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(CustomersModule.Extensions.ServiceCollectionExtensions).Assembly])
        .SwaggerDocument(s => s.ShortSchemaNames = true);

    // Add CustomersModule
    builder.Services.AddCustomersModule(builder.Configuration);

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Migrate database
    app.MigrateCustomersDatabase();

    app.UseExceptionHandler();
    app.UseHttpsRedirection();

    // Use observability (health checks, metrics, request logging)
    app.UseCustomersApiObservability();

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

    Log.Information("CustomersApi is ready");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CustomersApi terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
