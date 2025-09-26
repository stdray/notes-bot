using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Features.ContentExtraction.YouTubeContent;

/// <summary>
/// Обработчик извлеченного YouTube контента
/// Подготавливает контент для суммаризации
/// </summary>
public class YouTubeContentExtractedHandler(
  IBus bus,
  ILogger<YouTubeContentExtractedHandler> logger) : IHandleMessages<YouTubeContentExtracted>
{
  public async Task Handle(YouTubeContentExtracted message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(YouTubeContentExtracted), message.ChatId, message.CorrelationId);

    try
    {
      // Объединяем заголовок и описание для суммаризации
      var content = $"Title: {message.Title}\n\nDescription: {message.Description}";

      logger.LogInformation("Prepared YouTube content for summarization | {CorrelationId}",
        message.CorrelationId);

      // Отправляем контент для суммаризации
      await bus.Send(new ContentReadyForSummarization(
        content,
        message.Url,
        message.ChatId,
        message.MessageId,
        message.CorrelationId));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(YouTubeContentExtracted), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(YouTubeContentExtracted), message.ChatId, message.CorrelationId);
      throw;
    }
  }
}