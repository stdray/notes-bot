using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Features.ContentExtraction.YouTubeContent;

/// <summary>
/// Обработчик обнаруженных YouTube ссылок
/// Извлекает метаданные видео и отправляет для дальнейшей обработки
/// </summary>
public class YouTubeLinkDetectedHandler(
  IBus bus,
  ILogger<YouTubeLinkDetectedHandler> logger) : IHandleMessages<YouTubeLinkDetected>
{
  public async Task Handle(YouTubeLinkDetected message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(YouTubeLinkDetected), message.Meta.ChatId, message.Meta.CorrelationId);

    try
    {
      // TODO: Добавить валидацию URL
      
      // TODO: Извлечь ID видео из URL
      var videoId = ExtractVideoId(message.Url);
      
      // TODO: Получить метаданные через YouTube API
      // Пока заглушка
      var title = $"YouTube Video {videoId}";
      var description = "Video description will be extracted here";

      logger.LogInformation("Extracted YouTube content for video {VideoId} | {CorrelationId}",
        videoId, message.Meta.CorrelationId);

      // Отправляем извлеченный контент для дальнейшей обработки
      await bus.Send(new YouTubeContentExtracted(
        title,
        description,
        message.Url,
        message.Meta));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(YouTubeLinkDetected), message.Meta.ChatId, message.Meta.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(YouTubeLinkDetected), message.Meta.ChatId, message.Meta.CorrelationId);
      throw;
    }
  }

  private static string ExtractVideoId(string url)
  {
    // Простое извлечение ID видео из URL
    var uri = new Uri(url);
    
    if (uri.Host.Contains("youtu.be"))
    {
      return uri.AbsolutePath.TrimStart('/');
    }
    
    if (uri.Host.Contains("youtube.com"))
    {
      var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
      return query["v"] ?? "unknown";
    }
    
    return "unknown";
  }
}