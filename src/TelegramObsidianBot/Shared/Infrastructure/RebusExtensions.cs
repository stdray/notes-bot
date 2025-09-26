using Rebus.Config;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

namespace TelegramObsidianBot.Shared.Infrastructure;

public static class RebusExtensions
{
  public static IServiceCollection AddRebus(this IServiceCollection services)
  {
    // Регистрируем Rebus с in-memory транспортом для разработки
    services.AddRebus(configure => configure
      .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "telegram-obsidian-bot"))
      .Options(o => o.SetNumberOfWorkers(1))
      .Logging(l => l.Serilog()));

    return services;
  }

  public static IServiceCollection AddRebusHandlers(this IServiceCollection services)
  {
    // Автоматическая регистрация всех handler'ов из сборки
    services.AutoRegisterHandlersFromAssemblyOf<Program>();
    
    return services;
  }
}