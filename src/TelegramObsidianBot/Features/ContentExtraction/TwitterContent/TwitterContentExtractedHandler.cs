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
      nameof(TwitterContentExtracted), message.ChatId, message.CorrelationId);

    try
    {
      logger.LogInformation("Prepared Twitter content for summarization from {Url} | {CorrelationId}",
        message.Url, message.CorrelationId);

      // Отправляем контент для суммаризации
      await bus.Send(new ContentReadyForSummarization(
        message.Content,
        message.Url,
        message.ChatId,
        message.MessageId,
        message.CorrelationId));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterContentExtracted), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterContentExtracted), message.ChatId, message.CorrelationId);
      throw;
    }
  }
}