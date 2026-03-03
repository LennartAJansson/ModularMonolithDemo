using FastEndpoints;
using FastEndpoints.Swagger;
using MicroEvents;
using Scalar.AspNetCore;
using WorkloadsModule.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add MicroEvents (NATS-based event bus)
builder.Services.AddMicroEvents(builder.Configuration);

// Add FastEndpoints with WorkloadsModule assembly
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(WorkloadsModule.Extensions.ServiceCollectionExtensions).Assembly])
    .SwaggerDocument(s => s.ShortSchemaNames = true);

// Add WorkloadsModule
builder.Services.AddWorkloadsModule(builder.Configuration);

var app = builder.Build();

// Migrate database
app.MigrateWorkloadsDatabase();

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

app.UseHttpsRedirection();

app.Run();
