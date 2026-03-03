using CustomersModule.Extensions;

using FastEndpoints;
using FastEndpoints.Swagger;

using MicroEvents;

using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add MicroEvents (NATS-based event bus)
builder.Services.AddMicroEvents(builder.Configuration);

// Add FastEndpoints with CustomersModule assembly
builder.Services.AddFastEndpoints(o => o.Assemblies = [typeof(CustomersModule.Extensions.ServiceCollectionExtensions).Assembly])
    .SwaggerDocument(s => s.ShortSchemaNames = true);

// Add CustomersModule
builder.Services.AddCustomersModule(builder.Configuration);

var app = builder.Build();

// Migrate database
app.MigrateCustomersDatabase();

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
