namespace TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;

/// <summary>
/// Extension methods для регистрации сервисов обработки Telegram сообщений
/// </summary>
public static class TelegramMessageServiceExtensions
{
  public static IServiceCollection AddTelegramMessageFeature(this IServiceCollection services)
  {
    // Регистрируем hosted service для polling
    services.AddHostedService<TelegramBotHostedService>();
    
    // Регистрируем дополнительные сервисы если нужно
    // services.AddScoped<ITelegramMessageValidator, TelegramMessageValidator>();
    
    return services;
  }
}