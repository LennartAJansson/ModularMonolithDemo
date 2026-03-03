namespace MicroEvents;

using Events.Abstract;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class MicroEventsExtensions
{
  extension(IServiceCollection services)
  {
    public IServiceCollection AddMicroEvents(IConfiguration configuration)
      => services.AddSingleton<IEvents, MicroEvents>();
  }
}
