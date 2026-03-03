# Copilot Instructions

## General Overview
This repository contains an event-driven microservices solution using .NET 10 with two transport implementations:
- **ModularEvents**: In-process event bus using System.Threading.Channels
- **MicroEvents**: Out-of-process event bus using NATS JetStream

The solution follows Clean Architecture, SOLID principles, and vertical slice architecture patterns.

## Project Structure

### Event Infrastructure
* **Events.Abstract** - `IEvents` interface defining the event bus contract
* **ModularEvents** - In-process event transport using Channels
* **MicroEvents** - Out-of-process event transport using NATS JetStream

### Event Contracts
* **CustomersContract** - Customer events (Created, Updated, Deleted) as positional records
* **EmployeesContract** - Employee events (Created, Updated, Deleted) as positional records

### Modular Monolith
* **ModularApi** - Main API host for modular architecture
* **CustomersModule** - Customer domain module
* **EmployeesModule** - Employee domain module
* **WorkloadsModule** - Workloads domain module

### Microservices
* **CustomersApi** - Standalone customer API
* **EmployeesApi** - Standalone employee API
* **WorkloadsApi** - Standalone workloads API

## Architecture Patterns

### Event-Driven Communication
- Use `IEvents` interface for publishing and subscribing to events
- Events are immutable positional records
- Support both in-process (Channels) and out-of-process (NATS) transports
- All events implement semantic versioning through namespaced contracts

### Transport Selection
- **ModularEvents (Channels)**: Use for in-process, single-instance deployments
- **MicroEvents (NATS)**: Use for distributed, multi-instance deployments

### Event Naming Convention
Events follow the pattern: `{Entity}{Action}` (e.g., `CustomerCreated`, `EmployeeUpdated`)

## Technology Stack

### Framework
- **.NET 10** - Latest .NET version with C# 14 features
- **ASP.NET Core** - Web API framework
- **FastEndpoints** - Minimal API endpoints (if used)

### Messaging
- **System.Threading.Channels** - In-process async messaging
- **NATS JetStream** - Distributed messaging and event streaming
- **NATS.Client.JetStream** - Official .NET NATS client

### Data Access (if used in modules)
- **Entity Framework Core 10** - ORM
- **MySQL** - Database (use MySql.EntityFrameworkCore or Pomelo.EntityFrameworkCore.MySql)
- **Dapper** - Lightweight ORM for read operations (optional)

### Observability
- **OpenTelemetry** - Distributed tracing and metrics
- **Serilog** - Structured logging
- **Prometheus** - Metrics collection
- **Grafana** - Visualization
- **Loki** - Log aggregation
- **Jaeger** - Distributed tracing

## Development Guidelines

### Event Publishing
```csharp
// Single event
await events.PublishAsync(new CustomerCreated(
    Id: customerId,
    Name: "John Doe",
    Email: "john@example.com"
), cancellationToken);

// Multiple events
await events.PublishAsync(eventList, cancellationToken);
```

### Event Subscription
```csharp
// Subscribe to events
await foreach (var evt in events.SubscribeAsync<CustomerCreated>(cancellationToken))
{
    // Handle event
    await ProcessCustomerCreatedAsync(evt, cancellationToken);
}
```

### Event Contracts
- All events MUST be positional records
- Use init-only properties (primary constructor)
- Events are immutable
- Include all necessary data for subscribers
- Version events through namespace/assembly versioning

### Module Design
- Each module is self-contained with its own domain logic
- Modules communicate through events (IEvents)
- No direct module-to-module dependencies
- Each module can have its own database schema (if needed)

### API Design
- Use FastEndpoints for clean, testable endpoints
- Vertical slice architecture (feature folders)
- CQRS pattern where applicable
- Async/await throughout
- Proper cancellation token usage

## Clean Code Standards

### SOLID Principles
- **Single Responsibility**: Each class/record has one reason to change
- **Open/Closed**: Extend behavior through composition, not modification
- **Liskov Substitution**: Implementations are substitutable for abstractions
- **Interface Segregation**: Small, focused interfaces (e.g., IEvents)
- **Dependency Inversion**: Depend on abstractions (IEvents), not concrete types

### DRY, YAGNI, KISS
- **DRY**: Shared logic in libraries (Events.Abstract)
- **YAGNI**: Build only what's needed now
- **KISS**: Simple solutions over complex abstractions

## Testing Guidelines

### Unit Testing
- **xUnit** - Test framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library
- Test event publishing and subscription logic
- Mock IEvents for testing event producers
- Test event handlers independently

### Integration Testing
- **Testcontainers.Nats** - NATS integration testing
- Test end-to-end event flow
- Verify event serialization/deserialization
- Test error handling and retry logic

## NATS JetStream Patterns

### Stream Configuration
- Stream per entity type or bounded context
- Subject pattern: `events.{EntityName}.{Action}`
- Retention policies based on use case
- Persistent storage for reliability

### Consumer Configuration
- Durable consumers for reliable processing
- Explicit acknowledgment for at-least-once delivery
- Consumer groups for scaling
- Dead letter queues for poison messages

## Best Practices

### Event Design
- Events represent facts that happened (past tense)
- Include all data subscribers need (no additional lookups)
- Use Guid for entity IDs
- Include timestamps when relevant
- Version events through separate contracts/namespaces

### Error Handling
- Use try-catch in subscription loops
- Log errors but continue processing
- Implement dead letter queues for poison messages
- Use cancellation tokens for graceful shutdown

### Performance
- Use channels for high-throughput in-process scenarios
- Use NATS for distributed, resilient scenarios
- Batch events when possible
- Monitor queue depths and processing lag

### Security
- Validate event data before processing
- Don't trust event content implicitly
- Use authentication/authorization for NATS
- Encrypt sensitive data in events

## Observability

### Logging
- Log event publishing (info level)
- Log event processing start/end (debug level)
- Log errors with full context
- Use structured logging (Serilog)
- Include correlation IDs

### Metrics
- Track event publish rate
- Track event processing time
- Monitor queue depths
- Track error rates
- Alert on processing lag

### Tracing
- Trace event flow through system
- Correlate events across services
- Use OpenTelemetry for distributed tracing

## CI/CD Considerations

### Build
- Build all projects in solution
- Run unit tests
- Verify event contracts haven't broken
- Check code coverage

### Deploy
- Deploy modules as part of modular monolith
- Deploy microservices independently
- Ensure NATS infrastructure exists
- Run integration tests post-deployment

## Documentation
- Keep README.md updated with architecture decisions
- Document event schemas in contract projects
- Maintain CHANGELOG.md for breaking changes
- Document NATS stream/consumer configuration

## Notes
- This is a demo/learning project for event-driven architectures
- Focus on learning patterns, not production-grade features
- Experiment with both Channels and NATS transports
- Compare in-process vs distributed trade-offs
