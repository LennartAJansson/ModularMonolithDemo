namespace ModularEvents;

using Events.Abstract;

using Microsoft.Extensions.DependencyInjection;

public static class ModularEventsExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddModularEvents()
    {
      return services.AddSingleton<IEvents, ModularEvents>();
    }
  }
}
