using CustomersModule.Extensions;

using EmployeesModule.Extensions;

using FastEndpoints;
using FastEndpoints.Swagger;

using ModularEvents;

using Scalar.AspNetCore;

using WorkloadsModule.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add ModularEvents (in-process event bus)
builder.Services.AddModularEvents();

// Add FastEndpoints with all module assemblies
builder.Services.AddFastEndpoints(o => o.Assemblies = [
    typeof(CustomersModule.Extensions.ServiceCollectionExtensions).Assembly,
    typeof(EmployeesModule.Extensions.ServiceCollectionExtensions).Assembly,
    typeof(WorkloadsModule.Extensions.ServiceCollectionExtensions).Assembly
])
.SwaggerDocument(s => s.ShortSchemaNames = true);

// Add all modules
builder.Services.AddCustomersModule(builder.Configuration);
builder.Services.AddEmployeesModule(builder.Configuration);
builder.Services.AddWorkloadsModule(builder.Configuration);

var app = builder.Build();

// Migrate all databases
app.MigrateCustomersDatabase();
app.MigrateEmployeesDatabase();
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
