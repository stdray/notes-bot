using TelegramObsidianBot.Features.ContentExtraction.TwitterContent;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Tests;

/// <summary>
/// Тесты для валидатора Twitter URL
/// </summary>
public class TwitterUrlValidatorTests
{
  private readonly TwitterUrlValidator _validator = new();

  [Theory]
  [InlineData("https://twitter.com/user/status/123456789", true)]
  [InlineData("https://x.com/user/status/123456789", true)]
  [InlineData("https://example.com", false)]
  [InlineData("", false)]
  [InlineData(null, false)]
  public void Validate_TwitterUrl_ShouldReturnExpectedResult(string url, bool isValid)
  {
    // Arrange
    var message = new TwitterLinkDetected(url, 12345, "msg_123", "corr_456");

    // Act
    var result = _validator.Validate(message);

    // Assert
    Assert.Equal(isValid, result.IsValid);
  }

  [Fact]
  public void Validate_InvalidChatId_ShouldReturnError()
  {
    // Arrange
    var message = new TwitterLinkDetected("https://twitter.com/user/status/123", 0, "msg_123", "corr_456");

    // Act
    var result = _validator.Validate(message);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "ChatId");
  }

  [Fact]
  public void Validate_EmptyMessageId_ShouldReturnError()
  {
    // Arrange
    var message = new TwitterLinkDetected("https://twitter.com/user/status/123", 12345, "", "corr_456");

    // Act
    var result = _validator.Validate(message);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "MessageId");
  }

  [Fact]
  public void Validate_EmptyCorrelationId_ShouldReturnError()
  {
    // Arrange
    var message = new TwitterLinkDetected("https://twitter.com/user/status/123", 12345, "msg_123", "");

    // Act
    var result = _validator.Validate(message);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "CorrelationId");
  }

  [Fact]
  public void Validate_ValidMessage_ShouldPass()
  {
    // Arrange
    var message = new TwitterLinkDetected(
      "https://twitter.com/user/status/123456789", 
      12345, 
      "msg_123", 
      "corr_456");

    // Act
    var result = _validator.Validate(message);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
  }
}