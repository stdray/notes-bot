namespace TelegramObsidianBot.Shared.Infrastructure.Twitter;

/// <summary>
/// Интерфейс для работы с Twitter API
/// </summary>
public interface ITwitterClient
{
  /// <summary>
  /// Извлекает информацию о твите по URL
  /// </summary>
  Task<TwitterTweetInfo?> GetTweetAsync(string tweetUrl, CancellationToken cancellationToken = default);
}

/// <summary>
/// Информация о твите
/// </summary>
public record TwitterTweetInfo(
  string Id,
  string Text,
  string AuthorName,
  string AuthorUsername,
  DateTime CreatedAt,
  string[] MediaUrls,
  string[] ExtractedUrls);
