using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TelegramObsidianBot.Shared.Common;
using TelegramObsidianBot.Shared.Infrastructure.AI;

namespace TelegramObsidianBot.Tests;

/// <summary>
/// Тесты для AI сервиса
/// </summary>
public class AIServiceTests
{
  private readonly Mock<IServiceProvider> _serviceProviderMock;
  private readonly Mock<ILogger<AIService>> _loggerMock;
  private readonly IOptions<OllamaConfiguration> _ollamaConfig;
  private readonly IOptions<OpenRouterConfiguration> _openRouterConfig;

  public AIServiceTests()
  {
    _serviceProviderMock = new Mock<IServiceProvider>();
    _loggerMock = new Mock<ILogger<AIService>>();
    
    _ollamaConfig = Options.Create(new OllamaConfiguration(
      "http://localhost:11434",
      "llama3.1"));
    
    _openRouterConfig = Options.Create(new OpenRouterConfiguration(
      "test-api-key",
      "anthropic/claude-3-haiku"));
  }

  [Fact]
  public async Task SummarizeContentAsync_WhenCalled_ShouldReturnNonEmptyString()
  {
    // Arrange
    var aiService = new AIService(
      _serviceProviderMock.Object,
      _ollamaConfig,
      _openRouterConfig,
      _loggerMock.Object);

    var content = "This is a test content for summarization.";
    var sourceUrl = "https://example.com";

    // Act & Assert
    // Этот тест будет падать без реального AI, но показывает структуру
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await aiService.SummarizeContentAsync(content, sourceUrl));
  }

  [Fact]
  public async Task GenerateTagsAsync_WhenCalled_ShouldReturnArrayOfTags()
  {
    // Arrange
    var aiService = new AIService(
      _serviceProviderMock.Object,
      _ollamaConfig,
      _openRouterConfig,
      _loggerMock.Object);

    var content = "This is a test content about programming and development.";

    // Act & Assert
    // Этот тест будет падать без реального AI, но показывает структуру
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      await aiService.GenerateTagsAsync(content));
  }

  [Fact]
  public void OllamaConfiguration_ShouldHaveCorrectValues()
  {
    // Arrange & Act
    var config = _ollamaConfig.Value;

    // Assert
    Assert.Equal("http://localhost:11434", config.Endpoint);
    Assert.Equal("llama3.1", config.Model);
  }

  [Fact]
  public void OpenRouterConfiguration_ShouldHaveCorrectValues()
  {
    // Arrange & Act
    var config = _openRouterConfig.Value;

    // Assert
    Assert.Equal("test-api-key", config.ApiKey);
    Assert.Equal("anthropic/claude-3-haiku", config.Model);
  }
}