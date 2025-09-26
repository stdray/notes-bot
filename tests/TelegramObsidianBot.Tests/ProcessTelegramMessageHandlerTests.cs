using Microsoft.Extensions.Logging;
using Moq;
using Rebus.Bus;
using TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Tests;

/// <summary>
/// Тесты для ProcessTelegramMessageHandler
/// </summary>
public class ProcessTelegramMessageHandlerTests
{
  private readonly Mock<IBus> _busMock;
  private readonly Mock<ILogger<ProcessTelegramMessageHandler>> _loggerMock;
  private readonly ProcessTelegramMessageHandler _handler;

  public ProcessTelegramMessageHandlerTests()
  {
    _busMock = new Mock<IBus>();
    _loggerMock = new Mock<ILogger<ProcessTelegramMessageHandler>>();
    _handler = new ProcessTelegramMessageHandler(_busMock.Object, _loggerMock.Object);
  }

  [Fact]
  public async Task Handle_WhenYouTubeUrl_ShouldSendYouTubeLinkDetected()
  {
    // Arrange
    var message = new ProcessTelegramMessage(
      "Check this video: https://www.youtube.com/watch?v=dQw4w9WgXcQ",
      12345,
      "msg_123",
      "corr_456");

    // Act
    await _handler.Handle(message);

    // Assert
    _busMock.Verify(x => x.Send(
      It.Is<YouTubeLinkDetected>(cmd => 
        cmd.Url == "https://www.youtube.com/watch?v=dQw4w9WgXcQ" &&
        cmd.Meta.ChatId == 12345 &&
        cmd.Meta.MessageId == "msg_123" &&
        cmd.Meta.CorrelationId == "corr_456"),
      It.IsAny<Dictionary<string, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenTwitterUrl_ShouldSendTwitterLinkDetected()
  {
    // Arrange
    var message = new ProcessTelegramMessage(
      "Look at this tweet: https://twitter.com/user/status/123456789",
      12345,
      "msg_123",
      "corr_456");

    // Act
    await _handler.Handle(message);

    // Assert
    _busMock.Verify(x => x.Send(
      It.Is<TwitterLinkDetected>(cmd => 
        cmd.Url == "https://twitter.com/user/status/123456789"),
      It.IsAny<Dictionary<string, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenGitHubUrl_ShouldSendGitHubLinkDetected()
  {
    // Arrange
    var message = new ProcessTelegramMessage(
      "Check this repo: https://github.com/user/repository",
      12345,
      "msg_123",
      "corr_456");

    // Act
    await _handler.Handle(message);

    // Assert
    _busMock.Verify(x => x.Send(
      It.Is<GitHubLinkDetected>(cmd => 
        cmd.Url == "https://github.com/user/repository"),
      It.IsAny<Dictionary<string, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenRegularUrl_ShouldSendArticleLinkDetected()
  {
    // Arrange
    var message = new ProcessTelegramMessage(
      "Read this article: https://example.com/article",
      12345,
      "msg_123",
      "corr_456");

    // Act
    await _handler.Handle(message);

    // Assert
    _busMock.Verify(x => x.Send(
      It.Is<ArticleLinkDetected>(cmd => 
        cmd.Url == "https://example.com/article"),
      It.IsAny<Dictionary<string, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WhenEmptyMessage_ShouldNotSendAnyCommands()
  {
    // Arrange
    var message = new ProcessTelegramMessage(
      "",
      12345,
      "msg_123",
      "corr_456");

    // Act
    await _handler.Handle(message);

    // Assert
    _busMock.Verify(x => x.Send(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
      Times.Never);
  }
}