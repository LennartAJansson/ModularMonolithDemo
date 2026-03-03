---
description: .NET Web API development for microservices using MicroEvents (NATS JetStream)
applyTo: '**/CustomersApi/**/*.cs,**/EmployeesApi/**/*.cs,**/WorkloadsApi/**/*.cs,**/*.csproj'
---

# .NET Microservices API Development Guidelines

## Platform Rules (Strict)

- **Target: .NET 10** - Always use latest .NET
- **C# 14** - Latest language features
- **Event Transport: MicroEvents (NATS JetStream)** - Out-of-process events only

## Technology Stack

### Core Framework
- **.NET 10** - Latest .NET version
- **ASP.NET Core** - Web API framework
- **FastEndpoints** - Minimal API endpoints (if used)
- **C# 14** - Primary constructors, collection expressions

### Event Bus
- **MicroEvents** - NATS JetStream transport
- **NATS.Client.JetStream** - Official .NET client
- **Event Contracts** - CustomersContract, EmployeesContract

### Data Access
- **Entity Framework Core 10** - ORM
- **MySQL** - Database (use MySql.EntityFrameworkCore or Pomelo.EntityFrameworkCore.MySql)
- **Dapper** - Lightweight ORM for read operations (optional)

### Observability
- **OpenTelemetry** - Traces to Jaeger, metrics to Prometheus
- **Serilog** - Structured logging (console + Loki)
- **Prometheus.NET** - Metrics export at `/metrics`

## Microservices Architecture

### Separation of Concerns

**CustomersApi:**
- Publishes: `CustomerCreated`, `CustomerUpdated`, `CustomerDeleted`
- Subscribes: (if needed for cross-domain logic)
- Database: Customers schema in MySQL

**EmployeesApi:**
- Publishes: `EmployeeCreated`, `EmployeeUpdated`, `EmployeeDeleted`
- Subscribes: `CustomerCreated` (if employee linked to customer)
- Database: Employees schema in MySQL

**WorkloadsApi:**
- Subscribes: `CustomerCreated`, `EmployeeCreated`, etc.
- Aggregates events for workload management
- Database: Workloads schema in MySQL

### Event-Driven Communication

All APIs use **MicroEvents (NATS JetStream)** for inter-service communication:

```csharp
// Program.cs - CustomersApi
builder.Services.AddSingleton<INatsConnection>(sp =>
{
    var natsUrl = builder.Configuration["Nats:Url"] ?? "nats://nats.default.svc.cluster.local:4222";
    var natsConnection = new NatsConnection(new NatsOpts { Url = natsUrl });
    natsConnection.ConnectAsync().GetAwaiter().GetResult();
    return natsConnection;
});

builder.Services.AddSingleton<INatsJSContext>(sp =>
{
    var nats = sp.GetRequiredService<INatsConnection>();
    return new NatsJSContext(nats);
});

builder.Services.AddSingleton<IEvents, MicroEvents>();
```

### Publishing Events

```csharp
// CustomersApi - Create endpoint
public class CreateCustomerEndpoint(IEvents events, AppDbContext db) 
    : Endpoint<CreateCustomerRequest, CreateCustomerResponse>
{
    public override void Configure()
    {
        Post("/customers");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CreateCustomerRequest req, CancellationToken ct)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Email = req.Email,
            // ... other properties
        };
        
        db.Customers.Add(customer);
        await db.SaveChangesAsync(ct);
        
        // Publish event to NATS
        await events.PublishAsync(new CustomerCreated(
            Id: customer.Id,
            OrgNumber: customer.OrgNumber,
            Name: customer.Name,
            Email: customer.Email,
            PhoneNumber: customer.PhoneNumber,
            Address: customer.Address,
            PostalCode: customer.PostalCode,
            City: customer.City
        ), ct);
        
        await SendCreatedAtAsync<GetCustomerEndpoint>(
            new { id = customer.Id },
            new CreateCustomerResponse { Id = customer.Id, Name = customer.Name },
            cancellation: ct
        );
    }
}
```

### Subscribing to Events (Background Service)

```csharp
// WorkloadsApi - Event subscriber
public class CustomerEventsSubscriber(
    IEvents events,
    IServiceScopeFactory scopeFactory,
    ILogger<CustomerEventsSubscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CustomerEventsSubscriber started");
        
        await foreach (var evt in events.SubscribeAsync<CustomerCreated>(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ICustomerCreatedHandler>();
            
            try
            {
                await handler.HandleAsync(evt, stoppingToken);
                logger.LogInformation("Processed CustomerCreated event for {CustomerId}", evt.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing CustomerCreated event for {CustomerId}", evt.Id);
            }
        }
    }
}

// Register in Program.cs
builder.Services.AddHostedService<CustomerEventsSubscriber>();
builder.Services.AddScoped<ICustomerCreatedHandler, CustomerCreatedHandler>();
```

## EF Core 10+ & MySQL Configuration

### Setup with Retry Logic (CRITICAL for Kubernetes)

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => {
            // CRITICAL: Enable retry on failure for Kubernetes deployments
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
            mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
);
```

**Important**: Always enable `EnableRetryOnFailure()` for Kubernetes deployments to handle MySQL startup delays.

### DbContext

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
```

## OpenTelemetry Integration

### Setup (Program.cs)

```csharp
var serviceName = "CustomersApi";
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName, serviceVersion: serviceVersion)
        .AddAttributes([
            new("deployment.environment", builder.Environment.EnvironmentName),
            new("service.namespace", "MicroservicesDemo")
        ]))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource("NATS.Client")
        .AddSource(serviceName)
        .AddOtlpExporter(options => {
            options.Endpoint = new Uri("http://jaeger.monitoring.svc.cluster.local:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter(serviceName)
        .AddPrometheusExporter());

app.MapPrometheusScrapingEndpoint();
```

## Serilog Configuration

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CustomersApi")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
    .WriteTo.GrafanaLoki("http://loki.monitoring.svc.cluster.local:3100", 
        labels: new[] 
        {
            new LokiLabel { Key = "app", Value = "customersapi" },
            new LokiLabel { Key = "env", Value = builder.Environment.EnvironmentName }
        })
    .CreateLogger();

builder.Host.UseSerilog();
```

## Kubernetes Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database")
    .AddCheck("nats", () => 
    {
        var nats = builder.Services.BuildServiceProvider().GetRequiredService<INatsConnection>();
        return nats.ConnectionState == NatsConnectionState.Open 
            ? HealthCheckResult.Healthy() 
            : HealthCheckResult.Unhealthy();
    });

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => true
});
```

## Docker Configuration

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CustomersApi/CustomersApi.csproj", "CustomersApi/"]
COPY ["CustomersContract/CustomersContract.csproj", "CustomersContract/"]
COPY ["MicroEvents/MicroEvents.csproj", "MicroEvents/"]
COPY ["Events.Abstract/Events.Abstract.csproj", "Events.Abstract/"]
RUN dotnet restore "CustomersApi/CustomersApi.csproj"
COPY . .
WORKDIR "/src/CustomersApi"
RUN dotnet build "CustomersApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomersApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomersApi.dll"]
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql.mysql.svc.cluster.local;Database=customers;User=root;Password=CHANGE_ME"
  },
  "Nats": {
    "Url": "nats://nats.default.svc.cluster.local:4222"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## Testing

### Unit Testing

```csharp
using FluentAssertions;
using Moq;
using Xunit;

public class CreateCustomerEndpointTests
{
    private readonly Mock<IEvents> _eventsMock;
    private readonly AppDbContext _context;
    
    public CreateCustomerEndpointTests()
    {
        _eventsMock = new Mock<IEvents>();
        // Setup in-memory database for testing
    }
    
    [Fact]
    public async Task CreateCustomer_PublishesEvent()
    {
        // Arrange
        var endpoint = new CreateCustomerEndpoint(_eventsMock.Object, _context);
        var request = new CreateCustomerRequest { Name = "Test", Email = "test@test.com" };
        
        // Act
        await endpoint.HandleAsync(request, CancellationToken.None);
        
        // Assert
        _eventsMock.Verify(e => e.PublishAsync(
            It.Is<CustomerCreated>(evt => evt.Name == "Test"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### Integration Testing (with Testcontainers)

```csharp
using Testcontainers.Nats;
using Xunit;

public class CustomersApiIntegrationTests : IAsyncLifetime
{
    private NatsContainer? _natsContainer;
    
    public async Task InitializeAsync()
    {
        _natsContainer = new NatsBuilder()
            .WithImage("nats:latest")
            .WithCommand("--js")
            .Build();
        
        await _natsContainer.StartAsync();
    }
    
    [Fact]
    public async Task CreateCustomer_PublishesToNats()
    {
        // Test end-to-end event flow
    }
    
    public async Task DisposeAsync()
    {
        if (_natsContainer != null)
            await _natsContainer.DisposeAsync();
    }
}
```

## Best Practices

### Event Publishing
- ✅ Publish events AFTER database save
- ✅ Use transactions if needed
- ✅ Include all necessary data in events
- ✅ Log event publishing for observability

### Event Subscribing
- ✅ Use scoped services in handlers
- ✅ Handle errors gracefully (log but continue)
- ✅ Use cancellation tokens
- ✅ Implement idempotency

### Database
- ✅ Enable retry on failure for Kubernetes
- ✅ Use migrations for schema changes
- ✅ Separate schemas per service
- ✅ No cross-service database queries

### Observability
- ✅ Log structured data
- ✅ Include correlation IDs
- ✅ Expose metrics at /metrics
- ✅ Implement health checks

## Deployment to Kubernetes

See `kubernetes-management.instructions.md` for:
- Helm chart configuration
- NATS JetStream setup
- MySQL deployment
- Monitoring integration
- Network policies
