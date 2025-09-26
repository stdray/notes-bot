using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Features.ContentExtraction.TwitterContent;

/// <summary>
/// Обработчик извлеченного Twitter контента
/// Подготавливает контент для суммаризации
/// </summary>
public class TwitterContentExtractedHandler(
  IBus bus,
  ILogger<TwitterContentExtractedHandler> logger) : IHandleMessages<TwitterContentExtracted>
{
  public async Task Handle(TwitterContentExtracted message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(TwitterContentExtracted), message.Meta.ChatId, message.Meta.CorrelationId);

    try
    {
      logger.LogInformation("Prepared Twitter content for summarization from {Url} | {CorrelationId}",
        message.Url, message.Meta.CorrelationId);

      // Отправляем контент для суммаризации
      await bus.Send(new ContentReadyForSummarization(
        message.Content,
        message.Url,
        message.Meta));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterContentExtracted), message.Meta.ChatId, message.Meta.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterContentExtracted), message.Meta.ChatId, message.Meta.CorrelationId);
      throw;
    }
  }
}