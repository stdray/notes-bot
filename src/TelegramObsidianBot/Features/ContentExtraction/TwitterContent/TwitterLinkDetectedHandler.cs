using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;
using TelegramObsidianBot.Shared.Infrastructure.Twitter;

namespace TelegramObsidianBot.Features.ContentExtraction.TwitterContent;

/// <summary>
/// Обработчик обнаруженных Twitter ссылок
/// Извлекает контент твита и отправляет для дальнейшей обработки
/// </summary>
public class TwitterLinkDetectedHandler(
  ITwitterClient twitterClient,
  IBus bus,
  ILogger<TwitterLinkDetectedHandler> logger) : IHandleMessages<TwitterLinkDetected>
{
  public async Task Handle(TwitterLinkDetected message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(TwitterLinkDetected), message.ChatId, message.CorrelationId);

    try
    {
      // Извлекаем информацию о твите
      logger.LogInformation("Extracting Twitter content from {Url} | {CorrelationId}",
        message.Url, message.CorrelationId);

      var tweetInfo = await twitterClient.GetTweetAsync(message.Url);

      if (tweetInfo == null)
      {
        logger.LogWarning("Failed to extract Twitter content from {Url} | {CorrelationId}",
          message.Url, message.CorrelationId);
        return;
      }

      // Формируем контент для дальнейшей обработки
      var content = FormatTweetContent(tweetInfo);

      logger.LogInformation("Extracted Twitter content from tweet {TweetId} by @{Username} | {CorrelationId}",
        tweetInfo.Id, tweetInfo.AuthorUsername, message.CorrelationId);

      // Отправляем извлеченный контент
      await bus.Send(new TwitterContentExtracted(
        content,
        message.Url,
        message.ChatId,
        message.MessageId,
        message.CorrelationId));

      // Если в твите есть другие ссылки, обрабатываем их рекурсивно
      await ProcessEmbeddedUrls(tweetInfo.ExtractedUrls, message);

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterLinkDetected), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TwitterLinkDetected), message.ChatId, message.CorrelationId);
      throw;
    }
  }

  private static string FormatTweetContent(TwitterTweetInfo tweetInfo)
  {
    var content = $"Tweet by @{tweetInfo.AuthorUsername} ({tweetInfo.AuthorName})\n";
    content += $"Posted: {tweetInfo.CreatedAt:yyyy-MM-dd HH:mm:ss}\n\n";
    content += $"Content:\n{tweetInfo.Text}\n";

    if (tweetInfo.MediaUrls.Length > 0)
    {
      content += $"\nMedia:\n{string.Join("\n", tweetInfo.MediaUrls)}\n";
    }

    if (tweetInfo.ExtractedUrls.Length > 0)
    {
      content += $"\nLinks:\n{string.Join("\n", tweetInfo.ExtractedUrls)}\n";
    }

    return content;
  }

  private async Task ProcessEmbeddedUrls(string[] urls, TwitterLinkDetected originalMessage)
  {
    foreach (var url in urls)
    {
      logger.LogInformation("Processing embedded URL from tweet: {Url} | {CorrelationId}",
        url, originalMessage.CorrelationId);

      // Определяем тип ссылки и отправляем соответствующее сообщение
      if (IsYouTubeUrl(url))
      {
        await bus.Send(new YouTubeLinkDetected(
          url, originalMessage.ChatId, originalMessage.MessageId, originalMessage.CorrelationId));
      }
      else if (IsGitHubUrl(url))
      {
        await bus.Send(new GitHubLinkDetected(
          url, originalMessage.ChatId, originalMessage.MessageId, originalMessage.CorrelationId));
      }
      else
      {
        await bus.Send(new ArticleLinkDetected(
          url, originalMessage.ChatId, originalMessage.MessageId, originalMessage.CorrelationId));
      }
    }
  }

  private static bool IsYouTubeUrl(string url)
  {
    return url.Contains("youtube.com") || url.Contains("youtu.be");
  }

  private static bool IsGitHubUrl(string url)
  {
    return url.Contains("github.com");
  }
}