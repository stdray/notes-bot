using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using TelegramObsidianBot.Shared.Common;

namespace TelegramObsidianBot.Shared.Infrastructure.AI;

/// <summary>
/// Extension methods для регистрации AI сервисов
/// </summary>
public static class AIServiceExtensions
{
  public static IServiceCollection AddAIServices(this IServiceCollection services, IConfiguration configuration)
  {
    // Регистрируем Ollama клиент (приоритет)
    services.AddKeyedSingleton<OpenAIClient>("ollama", (provider, key) =>
    {
      try
      {
        var ollamaConfig = provider.GetRequiredService<IOptions<OllamaConfiguration>>().Value;
        return new OpenAIClient(
          new ApiKeyCredential("not-needed"), // Ollama не требует API ключ
          new OpenAIClientOptions
          {
            Endpoint = new Uri(ollamaConfig.Endpoint + "/v1")
          });
      }
      catch (Exception ex)
      {
        var logger = provider.GetRequiredService<ILogger<AIService>>();
        logger.LogWarning(ex, "Failed to create Ollama client, will use OpenRouter fallback");
        return null!;
      }
    });

    // Регистрируем OpenRouter клиент (fallback)
    services.AddSingleton<OpenAIClient>(provider =>
    {
      var openRouterConfig = provider.GetRequiredService<IOptions<OpenRouterConfiguration>>().Value;
      
      if (string.IsNullOrEmpty(openRouterConfig.ApiKey))
      {
        throw new InvalidOperationException("OpenRouter API key is required for fallback");
      }

      return new OpenAIClient(
        new ApiKeyCredential(openRouterConfig.ApiKey),
        new OpenAIClientOptions
        {
          Endpoint = new Uri("https://openrouter.ai/api/v1")
        });
    });

    // Регистрируем основной AI сервис
    services.AddScoped<IAIService, AIService>();

    return services;
  }
}