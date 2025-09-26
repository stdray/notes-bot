using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;
using TelegramObsidianBot.Shared.Infrastructure.AI;

namespace TelegramObsidianBot.Features.NoteGeneration.TagGeneration;

/// <summary>
/// Обработчик суммаризированного контента
/// Генерирует теги для заметки с помощью AI
/// </summary>
public class ContentSummarizedHandler(
  IAIService aiService,
  IBus bus,
  ILogger<ContentSummarizedHandler> logger) : IHandleMessages<ContentSummarized>
{
  public async Task Handle(ContentSummarized message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(ContentSummarized), message.ChatId, message.CorrelationId);

    try
    {
      // Используем AI для генерации тегов
      logger.LogInformation("Requesting AI tag generation for summarized content | {CorrelationId}",
        message.CorrelationId);

      var aiTags = await aiService.GenerateTagsAsync(message.Summary);
      
      // Добавляем базовые теги к AI тегам
      var allTags = CombineWithBasicTags(aiTags, message.Summary);

      logger.LogInformation("Generated {TagCount} tags (AI: {AITagCount}, Basic: {BasicTagCount}) | {CorrelationId}",
        allTags.Length, aiTags.Length, allTags.Length - aiTags.Length, message.CorrelationId);

      // Отправляем сгенерированные теги
      await bus.Send(new TagsGenerated(
        allTags,
        message.Summary,
        message.ChatId,
        message.MessageId,
        message.CorrelationId));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(ContentSummarized), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // При ошибке AI используем fallback на базовые теги
      logger.LogWarning(ex, "AI tag generation failed, using basic tags fallback | {CorrelationId}",
        message.CorrelationId);

      var basicTags = GenerateBasicTags(message.Summary);
      
      await bus.Send(new TagsGenerated(
        basicTags,
        message.Summary,
        message.ChatId,
        message.MessageId,
        message.CorrelationId));

      // Логирование завершения с fallback
      logger.LogInformation("Completed processing {MessageType} with basic tags fallback for ChatId {ChatId} | {CorrelationId}",
        nameof(ContentSummarized), message.ChatId, message.CorrelationId);
    }
  }

  private static string[] CombineWithBasicTags(string[] aiTags, string content)
  {
    var basicTags = GenerateBasicTags(content);
    var allTags = new List<string>(aiTags);
    
    // Добавляем базовые теги, которых нет в AI тегах
    foreach (var basicTag in basicTags)
    {
      if (!allTags.Contains(basicTag, StringComparer.OrdinalIgnoreCase))
      {
        allTags.Add(basicTag);
      }
    }

    return allTags.ToArray();
  }

  private static string[] GenerateBasicTags(string content)
  {
    var tags = new List<string> { "telegram-bot", "auto-generated" };

    // Простая логика определения тегов по ключевым словам
    var lowerContent = content.ToLowerInvariant();
    
    if (lowerContent.Contains("youtube") || lowerContent.Contains("video"))
      tags.Add("video");
    
    if (lowerContent.Contains("article") || lowerContent.Contains("blog"))
      tags.Add("article");
    
    if (lowerContent.Contains("github") || lowerContent.Contains("repository"))
      tags.Add("development");
    
    if (lowerContent.Contains("twitter") || lowerContent.Contains("tweet"))
      tags.Add("social-media");

    return tags.ToArray();
  }
}