
using TelegramObsidianBot.Shared.Infrastructure.Twitter;
namespace TelegramObsidianBot.Features.ContentExtraction.TwitterContent;

/// <summary>
/// Extension methods для регистрации сервисов Twitter интеграции
/// </summary>
public static class TwitterContentServiceExtensions
{
  public static IServiceCollection AddTwitterContentFeature(this IServiceCollection services)
  {
    // Регистрируем Twitter клиент
    services.AddScoped<ITwitterClient, TwitterClient>();
    
    return services;
  }
}