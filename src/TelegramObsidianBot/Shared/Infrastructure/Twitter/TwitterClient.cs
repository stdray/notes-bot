using System.Net;
using System.Text.RegularExpressions;
using LinqToTwitter.OAuth;
using Microsoft.Extensions.Options;
using TelegramObsidianBot.Shared.Common;

namespace TelegramObsidianBot.Shared.Infrastructure.Twitter;

/// <summary>
/// Клиент для работы с Twitter API через LinqToTwitter с поддержкой прокси
/// </summary>
public class TwitterClient(
  IOptions<TwitterCredentials> credentials,
  IOptions<ProxyOptions> proxyOptions,
  ILogger<TwitterClient> logger) : ITwitterClient
{
  private static readonly Regex TweetIdRegex = new(
    @"(?:twitter\.com|x\.com)/\w+/status/(\d+)",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  private static readonly Regex UrlRegex = new(
    @"https?://[^\s]+",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  public async Task<TwitterTweetInfo?> GetTweetAsync(string tweetUrl, CancellationToken cancellationToken = default)
  {
    try
    {
      var tweetId = ExtractTweetId(tweetUrl);
      if (string.IsNullOrEmpty(tweetId))
      {
        logger.LogWarning("Could not extract tweet ID from URL: {Url}", tweetUrl);
        return null;
      }

      logger.LogInformation("Fetching tweet {TweetId} from Twitter API", tweetId);

      // TODO: Реализовать реальный вызов Twitter API
      // Пока заглушка с правильной структурой
      var auth = CreateAuthorizer();
      
      // Имитируем асинхронную операцию
      await Task.Delay(100, cancellationToken);
      
      // Заглушка данных твита
      var mockTweetText = $"This is a mock tweet content for ID {tweetId}. " +
                         "Check out this link: https://example.com/article";
      
      var extractedUrls = ExtractUrls(mockTweetText);

      var tweetInfo = new TwitterTweetInfo(
        tweetId,
        mockTweetText,
        "Mock User",
        "mockuser",
        DateTime.UtcNow,
        [], // MediaUrls - пока пустой
        extractedUrls);

      logger.LogInformation("Successfully fetched tweet {TweetId} by @{Username} (MOCK DATA)", 
        tweetId, tweetInfo.AuthorUsername);

      return tweetInfo;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to fetch tweet from URL: {Url}", tweetUrl);
      return null;
    }
  }

  private IAuthorizer CreateAuthorizer()
  {
    var cred = credentials.Value;
    var proxy = proxyOptions.Value;

    var authorizer = new SingleUserAuthorizer
    {
      CredentialStore = new InMemoryCredentialStore
      {
        ConsumerKey = cred.ConsumerKey,
        ConsumerSecret = cred.ConsumerSecret,
        OAuthToken = cred.AccessToken,
        OAuthTokenSecret = cred.AccessTokenSecret
      }
    };

    // Добавляем прокси если настроен
    if (!string.IsNullOrEmpty(proxy.Host))
    {
      authorizer.Proxy = new WebProxy(proxy.Host, proxy.Port)
      {
        Credentials = new NetworkCredential(proxy.Username, proxy.Password)
      };
      
      logger.LogDebug("Using proxy {Host}:{Port} for Twitter API", proxy.Host, proxy.Port);
    }

    return authorizer;
  }

  private static string ExtractTweetId(string tweetUrl)
  {
    var match = TweetIdRegex.Match(tweetUrl);
    return match.Success ? match.Groups[1].Value : string.Empty;
  }

  private static string[] ExtractUrls(string text)
  {
    return UrlRegex.Matches(text)
      .Select(m => m.Value)
      .Where(url => !url.Contains("twitter.com") && !url.Contains("x.com"))
      .ToArray();
  }
}
