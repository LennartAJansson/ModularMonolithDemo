---
description: Event-driven architecture with ModularEvents (Channels) and MicroEvents (NATS JetStream)
applyTo: '**/*.cs,**/*.csproj'
---

# Event-Driven Development Guidelines

## Platform

- **.NET 10** - Latest .NET version
- **C# 14** - Latest language features

## Event Infrastructure

### IEvents Interface

Single abstraction for all event transports:

```csharp
public interface IEvents
{
  Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
  Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default) where TEvent : class;
  IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>(CancellationToken cancellationToken = default) where TEvent : class;
}
```

### Transport Implementations

**ModularEvents (Channels):**
- In-process event bus
- High performance, low latency
- Single instance deployments
- Dictionary<Type, Channel<T>>
- No external dependencies

**MicroEvents (NATS JetStream):**
- Out-of-process event bus
- Distributed deployments
- Multi-instance support
- Persistent event storage
- At-least-once delivery

## Event Contracts

### Positional Records

All events MUST be positional records:

```csharp
namespace CustomersContract;

public sealed record CustomerCreated(
  Guid Id,
  string OrgNumber,
  string Name,
  string Email,
  string PhoneNumber,
  string Address,
  string PostalCode,
  string City);
```

**Benefits:**
- Immutable by default
- Value equality
- Concise syntax
- Deconstruction support

### Event Naming

Pattern: `{Entity}{Action}` (past tense)

```csharp
CustomerCreated    // ✅ Good
EmployeeUpdated    // ✅ Good
OrderDeleted       // ✅ Good

CreateCustomer     // ❌ Bad (command, not event)
Customer           // ❌ Bad (not descriptive)
```

### Contract Projects

- **CustomersContract** - Customer domain events
- **EmployeesContract** - Employee domain events
- Separate assembly per bounded context
- Shared between publishers and subscribers

## ModularEvents Implementation

### In-Process Event Bus

```csharp
public sealed class ModularEvents : IEvents
{
  private readonly Dictionary<Type, object> _channels = [];
  private readonly object _lock = new();

  public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    var channel = GetOrCreateChannel<TEvent>();
    return channel.Writer.WriteAsync(@event, cancellationToken).AsTask();
  }

  public async IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    where TEvent : class
  {
    var channel = GetOrCreateChannel<TEvent>();
    await foreach (var @event in channel.Reader.ReadAllAsync(cancellationToken))
    {
      yield return @event;
    }
  }

  private Channel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : class
  {
    lock (_lock)
    {
      if (!_channels.TryGetValue(typeof(TEvent), out var channelObj))
      {
        var channel = Channel.CreateUnbounded<TEvent>();
        _channels[typeof(TEvent)] = channel;
        return channel;
      }
      return (Channel<TEvent>)channelObj;
    }
  }
}
```

**Use for:**
- Modular monolith
- Single instance deployments
- High-throughput scenarios
- Development/testing

## MicroEvents Implementation

### NATS JetStream Event Bus

```csharp
public sealed class MicroEvents : IEvents, IAsyncDisposable
{
  private readonly NatsJSContext _jetStream;
  private readonly JsonSerializerOptions _jsonOptions;

  public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    var subject = GetSubject<TEvent>();
    var data = JsonSerializer.SerializeToUtf8Bytes(@event, _jsonOptions);
    await _jetStream.PublishAsync(subject, data, cancellationToken: cancellationToken);
  }

  public async IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    where TEvent : class
  {
    var streamName = GetStreamName<TEvent>();
    var consumer = await _jetStream.CreateOrUpdateConsumerAsync(streamName, config, cancellationToken);
    
    await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: cancellationToken))
    {
      var @event = await DeserializeEventAsync<TEvent>(msg, cancellationToken);
      if (@event is not null) yield return @event;
    }
  }

  private static string GetSubject<TEvent>() => $"events.{typeof(TEvent).Name}";
  private static string GetStreamName<TEvent>() => $"{typeof(TEvent).Name}Stream";
}
```

**Use for:**
- Microservices
- Multi-instance deployments
- Distributed systems
- Persistent event log

## Publishing Events

### Single Event

```csharp
await events.PublishAsync(new CustomerCreated(
    Id: Guid.NewGuid(),
    OrgNumber: "123456-7890",
    Name: "Acme Corp",
    Email: "info@acme.com",
    PhoneNumber: "+46701234567",
    Address: "Main Street 1",
    PostalCode: "12345",
    City: "Stockholm"
), cancellationToken);
```

### Multiple Events

```csharp
var events = new List<CustomerCreated>
{
    new(Guid.NewGuid(), "123", "Customer 1", ...),
    new(Guid.NewGuid(), "124", "Customer 2", ...)
};

await eventBus.PublishAsync(events, cancellationToken);
```

## Subscribing to Events

### Basic Subscription

```csharp
await foreach (var evt in events.SubscribeAsync<CustomerCreated>(cancellationToken))
{
    Console.WriteLine($"Customer created: {evt.Name}");
}
```

### Event Handler Pattern

```csharp
public class CustomerCreatedHandler
{
    private readonly IEvents _events;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await foreach (var evt in _events.SubscribeAsync<CustomerCreated>(cancellationToken))
        {
            await HandleAsync(evt, cancellationToken);
        }
    }
    
    private async Task HandleAsync(CustomerCreated evt, CancellationToken cancellationToken)
    {
        // Process event
        await Task.CompletedTask;
    }
}
```

## NATS Configuration

### Stream Setup

```csharp
await _jetStream.CreateStreamAsync(new StreamConfig
{
    Name = "CustomerStream",
    Subjects = ["events.CustomerCreated", "events.CustomerUpdated", "events.CustomerDeleted"],
    Retention = StreamConfigRetention.WorkQueue,
    Storage = StreamConfigStorage.File
}, cancellationToken);
```

### Consumer Setup

```csharp
var consumer = await _jetStream.CreateOrUpdateConsumerAsync(
    "CustomerStream",
    new ConsumerConfig
    {
        DurableName = "CustomerProcessor",
        AckPolicy = ConsumerConfigAckPolicy.Explicit,
        AckWait = TimeSpan.FromSeconds(30),
        MaxDeliver = 5
    },
    cancellationToken);
```

### Acknowledgment

```csharp
await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken))
{
    try
    {
        var evt = Deserialize<CustomerCreated>(msg.Data);
        await ProcessAsync(evt, cancellationToken);
        await msg.AckAsync(cancellationToken: cancellationToken);  // Success
    }
    catch
    {
        await msg.NakAsync(delay: TimeSpan.FromSeconds(5), cancellationToken: cancellationToken);  // Retry
    }
}
```

## Error Handling

### Yield Return Constraint

❌ **Cannot use `yield return` in try-catch blocks**

```csharp
// ❌ WRONG - Compile error
await foreach (var msg in consumer.ConsumeAsync<byte[]>())
{
    try
    {
        var evt = Deserialize(msg.Data);
        yield return evt;  // CS1626 error
    }
    catch { }
}
```

✅ **Extract to separate method:**

```csharp
// ✅ CORRECT
await foreach (var msg in consumer.ConsumeAsync<byte[]>())
{
    var evt = await DeserializeEventAsync(msg, cancellationToken);
    if (evt is not null) yield return evt;
}

private async Task<TEvent?> DeserializeEventAsync(INatsJSMsg<byte[]> msg, CancellationToken ct)
{
    try
    {
        var evt = JsonSerializer.Deserialize<TEvent>(msg.Data);
        await msg.AckAsync(cancellation: ct);
        return evt;
    }
    catch (JsonException)
    {
        await msg.AckAsync(cancellation: ct);  // ACK to prevent infinite retry
        return null;
    }
}
```

## Best Practices

### Event Design
- ✅ Events are facts (past tense)
- ✅ Include all necessary data
- ✅ No database lookups in subscribers
- ✅ Use Guid for IDs
- ✅ Immutable positional records

### Performance
- ✅ Use Channels for high-throughput in-process
- ✅ Use NATS for distributed reliability
- ✅ Batch events when possible
- ✅ Monitor queue depths

### Error Handling
- ✅ Log errors but continue processing
- ✅ Use cancellation tokens
- ✅ Implement dead letter queues
- ✅ ACK poison messages (don't retry forever)

### Testing
- ✅ Mock IEvents for unit tests
- ✅ Use Testcontainers.Nats for integration tests
- ✅ Test event serialization
- ✅ Verify at-least-once delivery

## Module Communication

### In-Process (ModularEvents)

```
ModularApi
  ├── CustomersModule → PublishAsync<CustomerCreated>
  ├── EmployeesModule → SubscribeAsync<CustomerCreated>
  └── WorkloadsModule → SubscribeAsync<CustomerCreated>
```

### Distributed (MicroEvents)

```
CustomersApi → NATS → EmployeesApi
                   └→ WorkloadsApi
```

## NATS Subject Patterns

- `events.{EntityName}.*` - All events for entity
- `events.{EntityName}.Created` - Specific event type
- `events.>` - All events (wildcard)

## Observability

### Logging

```csharp
await _logger.LogInformation("Publishing {EventType}", typeof(TEvent).Name);
await events.PublishAsync(@event, cancellationToken);
```

### Metrics

```csharp
_eventsPublished.Add(1, new KeyValuePair<string, object?>("event_type", typeof(TEvent).Name));
```

### Tracing

```csharp
using var activity = ActivitySource.StartActivity("PublishEvent");
activity?.SetTag("event.type", typeof(TEvent).Name);
```

## Common Patterns

### Saga Pattern
- Coordinate multi-step workflows
- Use events for state transitions
- Handle compensating transactions

### Event Sourcing
- Store events as source of truth
- Rebuild state from event log
- Audit trail included

### CQRS
- Commands → Write API → Events
- Events → Read Model
- Separate read/write concerns
