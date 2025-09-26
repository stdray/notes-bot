using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;
using TelegramObsidianBot.Shared.Infrastructure.AI;

namespace TelegramObsidianBot.Features.NoteGeneration.ContentSummarization;

/// <summary>
/// Обработчик контента готового для суммаризации
/// Создает краткое содержание с помощью AI
/// </summary>
public class ContentReadyForSummarizationHandler(
  IAIService aiService,
  IBus bus,
  ILogger<ContentReadyForSummarizationHandler> logger) : IHandleMessages<ContentReadyForSummarization>
{
  public async Task Handle(ContentReadyForSummarization message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(ContentReadyForSummarization), message.Meta.ChatId, message.Meta.CorrelationId);

    try
    {
      // Используем AI для создания суммаризации
      logger.LogInformation("Requesting AI summarization for content from {SourceUrl} | {CorrelationId}",
        message.SourceUrl, message.Meta.CorrelationId);

      var summary = await aiService.SummarizeContentAsync(
        message.Content, 
        message.SourceUrl);

      logger.LogInformation("Generated AI summary ({Length} chars) for content from {SourceUrl} | {CorrelationId}",
        summary.Length, message.SourceUrl, message.Meta.CorrelationId);

      // Отправляем суммаризированный контент
      await bus.Send(new ContentSummarized(
        summary,
        message.Content,
        message.Meta));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(ContentReadyForSummarization), message.Meta.ChatId, message.Meta.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(ContentReadyForSummarization), message.Meta.ChatId, message.Meta.CorrelationId);
      throw;
    }
  }
}