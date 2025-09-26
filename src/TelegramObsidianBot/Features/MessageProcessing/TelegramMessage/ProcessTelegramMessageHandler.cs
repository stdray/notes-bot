using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;
using System.Text.RegularExpressions;

namespace TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;

/// <summary>
/// Обработчик входящих сообщений из Telegram
/// Определяет тип сообщения и отправляет соответствующие команды
/// </summary>
public class ProcessTelegramMessageHandler(
  IBus bus,
  ILogger<ProcessTelegramMessageHandler> logger) : IHandleMessages<ProcessTelegramMessage>
{
  // Регулярные выражения для определения типов ссылок
  private static readonly Regex YouTubeRegex = new(
    @"(?:https?://)?(?:www\.)?(?:youtube\.com/watch\?v=|youtu\.be/)([a-zA-Z0-9_-]{11})",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  private static readonly Regex TwitterRegex = new(
    @"(?:https?://)?(?:www\.)?(?:twitter\.com|x\.com)/\w+/status/\d+",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  private static readonly Regex GitHubRegex = new(
    @"(?:https?://)?(?:www\.)?github\.com/[\w\-\.]+/[\w\-\.]+",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  private static readonly Regex UrlRegex = new(
    @"https?://[^\s]+",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

  public async Task Handle(ProcessTelegramMessage message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(ProcessTelegramMessage), message.ChatId, message.CorrelationId);

    try
    {
      // Валидация сообщения
      if (string.IsNullOrWhiteSpace(message.Text))
      {
        logger.LogInformation("Empty message received from ChatId {ChatId} | {CorrelationId}",
          message.ChatId, message.CorrelationId);
        return;
      }

      // Определяем тип контента и отправляем соответствующие команды
      await DetectAndProcessLinks(message);

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(ProcessTelegramMessage), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(ProcessTelegramMessage), message.ChatId, message.CorrelationId);
      throw;
    }
  }

  private async Task DetectAndProcessLinks(ProcessTelegramMessage message)
  {
    var urls = UrlRegex.Matches(message.Text).Select(m => m.Value).ToArray();

    if (urls.Length == 0)
    {
      logger.LogInformation("No URLs found in message from ChatId {ChatId} | {CorrelationId}",
        message.ChatId, message.CorrelationId);
      return;
    }

    foreach (var url in urls)
    {
      await ProcessUrl(url, message);
    }
  }

  private async Task ProcessUrl(string url, ProcessTelegramMessage message)
  {
    logger.LogInformation("Processing URL: {Url} | {CorrelationId}", url, message.CorrelationId);

    // YouTube ссылки
    if (YouTubeRegex.IsMatch(url))
    {
      await bus.Send(new YouTubeLinkDetected(
        url, new MessageMeta(message.ChatId, message.MessageId, message.CorrelationId)));
      return;
    }

    // Twitter/X ссылки
    if (TwitterRegex.IsMatch(url))
    {
      await bus.Send(new TwitterLinkDetected(
        url, new MessageMeta(message.ChatId, message.MessageId, message.CorrelationId)));
      return;
    }

    // GitHub ссылки
    if (GitHubRegex.IsMatch(url))
    {
      await bus.Send(new GitHubLinkDetected(
        url, new MessageMeta(message.ChatId, message.MessageId, message.CorrelationId)));
      return;
    }

    // Обычные статьи/веб-страницы
    await bus.Send(new ArticleLinkDetected(
      url, new MessageMeta(message.ChatId, message.MessageId, message.CorrelationId)));
  }
}