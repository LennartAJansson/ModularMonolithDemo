namespace MicroEvents;

using System.Runtime.CompilerServices;
using System.Text.Json;

using Events.Abstract;

using Microsoft.Extensions.Configuration;

using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

public sealed class MicroEvents
  : IEvents, IAsyncDisposable
{
  private readonly NatsConnection _natsConnection;
  private readonly NatsJSContext _jetStream;
  private readonly JsonSerializerOptions _jsonOptions;

  public MicroEvents(IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    // Read NATS URL from configuration with fallback
    string natsUrl = configuration["Nats:Url"] ?? "nats://localhost:4222";

    // Create NATS connection
    var natsOpts = new NatsOpts { Url = natsUrl };
    _natsConnection = new NatsConnection(natsOpts);
    _natsConnection.ConnectAsync().GetAwaiter().GetResult();

    // Create JetStream context
    _jetStream = new NatsJSContext(_natsConnection);

    _jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
  }

  public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    ArgumentNullException.ThrowIfNull(@event);

    string subject = GetSubject<TEvent>();
    byte[] data = JsonSerializer.SerializeToUtf8Bytes(@event, _jsonOptions);

    _=await _jetStream.PublishAsync(subject, data, cancellationToken: cancellationToken);
  }

  public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    ArgumentNullException.ThrowIfNull(events);

    string subject = GetSubject<TEvent>();
    foreach (var @event in events)
    {
      byte[] data = JsonSerializer.SerializeToUtf8Bytes(@event, _jsonOptions);
      _=await _jetStream.PublishAsync(subject, data, cancellationToken: cancellationToken);
    }
  }

  public async IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    where TEvent : class
  {
    string subject = GetSubject<TEvent>();
    string streamName = GetStreamName<TEvent>();

    await EnsureStreamExistsAsync<TEvent>(streamName, subject, cancellationToken);

    var consumer = await _jetStream.CreateOrUpdateConsumerAsync(
      streamName,
      new ConsumerConfig
      {
        Name = $"{typeof(TEvent).Name}Consumer",
        FilterSubject = subject,
        AckPolicy = ConsumerConfigAckPolicy.Explicit
      },
      cancellationToken);

    await foreach (var msg in consumer.ConsumeAsync<byte[]>(cancellationToken: cancellationToken))
    {
      var @event = await DeserializeEventAsync<TEvent>(msg, cancellationToken);
      if (@event is not null)
      {
        yield return @event;
      }
    }
  }

  private async Task<TEvent?> DeserializeEventAsync<TEvent>(INatsJSMsg<byte[]> msg, CancellationToken cancellationToken)
    where TEvent : class
  {
    try
    {
      var @event = JsonSerializer.Deserialize<TEvent>(msg.Data, _jsonOptions);
      await msg.AckAsync(cancellationToken: cancellationToken);
      return @event;
    }
    catch (JsonException)
    {
      await msg.AckAsync(cancellationToken: cancellationToken);
      return null;
    }
  }

  private async Task EnsureStreamExistsAsync<TEvent>(string streamName, string subject, CancellationToken cancellationToken)
    where TEvent : class
  {
    try
    {
      _=await _jetStream.GetStreamAsync(streamName, cancellationToken: cancellationToken);
    }
    catch
    {
      _=await _jetStream.CreateStreamAsync(
        new StreamConfig
        {
          Name = streamName,
          Subjects = [subject]
        },
        cancellationToken);
    }
  }

  private static string GetSubject<TEvent>() where TEvent : class
  {
    return $"events.{typeof(TEvent).Name}";
  }

  private static string GetStreamName<TEvent>() where TEvent : class
  {
    return $"{typeof(TEvent).Name}Stream";
  }

  public async ValueTask DisposeAsync()
  {
    // Dispose JetStream context first
    if (_jetStream is IAsyncDisposable jetStreamDisposable)
    {
      await jetStreamDisposable.DisposeAsync();
    }

    // Then dispose NATS connection
    if (_natsConnection is IAsyncDisposable connectionDisposable)
    {
      await connectionDisposable.DisposeAsync();
    }
  }
}
