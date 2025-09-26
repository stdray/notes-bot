using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TelegramObsidianBot.Shared.Infrastructure.Twitter;
using TelegramObsidianBot.Shared.Common;

namespace TelegramObsidianBot.Tests;

/// <summary>
/// Тесты для Twitter интеграции
/// </summary>
public class TwitterContentTests
{
  private readonly Mock<IOptions<TwitterCredentials>> _credentialsMock;
  private readonly Mock<IOptions<ProxyOptions>> _proxyMock;
  private readonly Mock<ILogger<TwitterClient>> _loggerMock;

  public TwitterContentTests()
  {
    _credentialsMock = new Mock<IOptions<TwitterCredentials>>();
    _proxyMock = new Mock<IOptions<ProxyOptions>>();
  _loggerMock = new Mock<ILogger<TwitterClient>>();

    _credentialsMock.Setup(x => x.Value).Returns(new TwitterCredentials(
      "test-consumer-key",
      "test-consumer-secret", 
      "test-access-token",
      "test-access-token-secret"));

    _proxyMock.Setup(x => x.Value).Returns(new ProxyOptions(
      "proxy.example.com",
      8080,
      "proxy-user",
      "proxy-pass"));
  }

  [Fact]
  public void TwitterCredentials_ShouldHaveCorrectValues()
  {
    // Arrange & Act
    var credentials = _credentialsMock.Object.Value;

    // Assert
    Assert.Equal("test-consumer-key", credentials.ConsumerKey);
    Assert.Equal("test-consumer-secret", credentials.ConsumerSecret);
    Assert.Equal("test-access-token", credentials.AccessToken);
    Assert.Equal("test-access-token-secret", credentials.AccessTokenSecret);
  }

  [Fact]
  public void ProxyOptions_ShouldHaveCorrectValues()
  {
    // Arrange & Act
    var proxy = _proxyMock.Object.Value;

    // Assert
    Assert.Equal("proxy.example.com", proxy.Host);
    Assert.Equal(8080, proxy.Port);
    Assert.Equal("proxy-user", proxy.Username);
    Assert.Equal("proxy-pass", proxy.Password);
  }

  [Theory]
  [InlineData("https://twitter.com/user/status/123456789", "123456789")]
  [InlineData("https://x.com/user/status/987654321", "987654321")]
  [InlineData("https://twitter.com/elonmusk/status/1234567890123456789", "1234567890123456789")]
  [InlineData("invalid-url", "")]
  [InlineData("https://example.com", "")]
  public void ExtractTweetId_ShouldReturnCorrectId(string url, string expectedId)
  {
    // Arrange
  var client = new TwitterClient(_credentialsMock.Object, _proxyMock.Object, _loggerMock.Object);

    // Act
    var result = ExtractTweetIdViaReflection(client, url);

    // Assert
    Assert.Equal(expectedId, result);
  }

  [Theory]
  [InlineData("Check this out: https://example.com and https://github.com/user/repo", 
    new[] { "https://example.com", "https://github.com/user/repo" })]
  [InlineData("No links here", new string[0])]
  [InlineData("Twitter link https://twitter.com/user/status/123 should be ignored", new string[0])]
  public void ExtractUrls_ShouldReturnCorrectUrls(string text, string[] expectedUrls)
  {
    // Arrange
  var client = new TwitterClient(_credentialsMock.Object, _proxyMock.Object, _loggerMock.Object);

    // Act
    var result = ExtractUrlsViaReflection(client, text);

    // Assert
    Assert.Equal(expectedUrls, result);
  }

  // Helper methods to test private methods via reflection
  private static string ExtractTweetIdViaReflection(TwitterClient client, string url)
  {
    var method = typeof(TwitterClient).GetMethod("ExtractTweetId", 
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    return (string)method!.Invoke(null, new object[] { url })!;
  }

  private static string[] ExtractUrlsViaReflection(TwitterClient client, string text)
  {
    var method = typeof(TwitterClient).GetMethod("ExtractUrls", 
      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    return (string[])method!.Invoke(null, new object[] { text })!;
  }
}