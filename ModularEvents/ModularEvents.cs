namespace ModularEvents;

using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Events.Abstract;

public sealed class ModularEvents
  : IEvents
{
  private readonly Dictionary<Type, object> _channels = [];
  private readonly object _lock = new();

  public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    ArgumentNullException.ThrowIfNull(@event);

    Channel<TEvent> channel = GetOrCreateChannel<TEvent>();
    return channel.Writer.WriteAsync(@event, cancellationToken).AsTask();
  }

  public async Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
    where TEvent : class
  {
    ArgumentNullException.ThrowIfNull(events);

    Channel<TEvent> channel = GetOrCreateChannel<TEvent>();
    foreach (TEvent @event in events)
    {
      await channel.Writer.WriteAsync(@event, cancellationToken);
    }
  }

  public async IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>([EnumeratorCancellation] CancellationToken cancellationToken = default)
    where TEvent : class
  {
    Channel<TEvent> channel = GetOrCreateChannel<TEvent>();
    await foreach (TEvent @event in channel.Reader.ReadAllAsync(cancellationToken))
    {
      yield return @event;
    }
  }

  private Channel<TEvent> GetOrCreateChannel<TEvent>() where TEvent : class
  {
    Type eventType = typeof(TEvent);

    lock (_lock)
    {
      if (!_channels.TryGetValue(eventType, out var channelObj))
      {
        var channel = Channel.CreateUnbounded<TEvent>(new UnboundedChannelOptions
        {
          SingleReader = false,
          SingleWriter = false
        });
        _channels[eventType] = channel;
        return channel;
      }

      return (Channel<TEvent>)channelObj;
    }
  }
}
