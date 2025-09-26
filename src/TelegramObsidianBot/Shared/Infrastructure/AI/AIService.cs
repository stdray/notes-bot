using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using TelegramObsidianBot.Shared.Common;

namespace TelegramObsidianBot.Shared.Infrastructure.AI;

/// <summary>
/// AI сервис с fallback: Ollama → OpenRouter
/// </summary>
public class AIService(
  IServiceProvider serviceProvider,
  IOptions<OllamaConfiguration> ollamaConfig,
  IOptions<OpenRouterConfiguration> openRouterConfig,
  ILogger<AIService> logger) : IAIService
{
  public async Task<string> SummarizeContentAsync(string content, string sourceUrl, CancellationToken cancellationToken = default)
  {
    var prompt = $"""
      Please create a concise summary of the following content from {sourceUrl}.
      Focus on the main points and key information.
      Keep the summary under 300 words.
      
      Content:
      {content}
      """;

    return await GetAIResponseAsync(prompt, "summarization", cancellationToken);
  }

  public async Task<string[]> GenerateTagsAsync(string content, CancellationToken cancellationToken = default)
  {
    var prompt = $"""
      Generate 3-7 relevant tags for the following content.
      Tags should be lowercase, single words or hyphenated phrases.
      Return only the tags separated by commas, no additional text.
      
      Content:
      {content}
      """;

    var response = await GetAIResponseAsync(prompt, "tag generation", cancellationToken);
    
    // Парсим теги из ответа
    return response
      .Split(',', StringSplitOptions.RemoveEmptyEntries)
      .Select(tag => tag.Trim().ToLowerInvariant())
      .Where(tag => !string.IsNullOrEmpty(tag))
      .ToArray();
  }

  private async Task<string> GetAIResponseAsync(string prompt, string operation, CancellationToken cancellationToken)
  {
    // Сначала пробуем Ollama
    try
    {
      logger.LogInformation("Attempting {Operation} with Ollama", operation);
      var ollamaClient = serviceProvider.GetKeyedService<OpenAIClient>("ollama");
      
      if (ollamaClient != null)
      {
        var response = await GetChatResponseAsync(ollamaClient, prompt, ollamaConfig.Value.Model, cancellationToken);
        logger.LogInformation("Successfully completed {Operation} with Ollama", operation);
        return response;
      }
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Ollama failed for {Operation}, falling back to OpenRouter", operation);
    }

    // Fallback на OpenRouter
    try
    {
      logger.LogInformation("Attempting {Operation} with OpenRouter", operation);
      var openRouterClient = serviceProvider.GetRequiredService<OpenAIClient>();
      
      var response = await GetChatResponseAsync(openRouterClient, prompt, openRouterConfig.Value.Model, cancellationToken);
      logger.LogInformation("Successfully completed {Operation} with OpenRouter", operation);
      return response;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Both Ollama and OpenRouter failed for {Operation}", operation);
      throw new InvalidOperationException($"AI {operation} failed: {ex.Message}", ex);
    }
  }

  private static async Task<string> GetChatResponseAsync(OpenAIClient client, string prompt, string model, CancellationToken cancellationToken)
  {
    var chatClient = client.GetChatClient(model);
    
    var messages = new List<ChatMessage>
    {
      new SystemChatMessage("You are a helpful assistant that provides concise and accurate responses."),
      new UserChatMessage(prompt)
    };

    var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

    return response.Value.Content[0].Text;
  }
}