namespace Events.Abstract;

public interface IEvents
{
  Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
    where TEvent : class;
  Task PublishAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default) 
    where TEvent : class;
  IAsyncEnumerable<TEvent> SubscribeAsync<TEvent>(CancellationToken cancellationToken = default) 
    where TEvent : class;
}
