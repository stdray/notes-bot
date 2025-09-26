using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramObsidianBot.Shared.Common;
using Rebus.Bus;

namespace TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;

/// <summary>
/// Hosted service для получения обновлений от Telegram через polling
/// </summary>
public class TelegramBotHostedService(
  IOptions<TelegramConfiguration> config,
  IBus bus,
  ILogger<TelegramBotHostedService> logger) : BackgroundService
{
  private readonly TelegramBotClient _client = new(config.Value.BotToken);

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Starting Telegram Bot polling");

    try
    {
      var me = await _client.GetMe(stoppingToken);
      logger.LogInformation("Bot started: @{Username} ({FirstName})", me.Username, me.FirstName);

      var receiverOptions = new ReceiverOptions
      {
        AllowedUpdates = [UpdateType.Message],
        DropPendingUpdates = true
      };

      _client.StartReceiving(
        HandleUpdateAsync,
        HandleErrorAsync,
        receiverOptions,
        stoppingToken);

      // Ждем до отмены
      await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Telegram Bot polling stopped");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in Telegram Bot polling");
      throw;
    }
  }

  private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
  {
    if (update.Message is not { } message)
      return;

    // Генерируем CorrelationId для трассировки
    var correlationId = Guid.NewGuid().ToString("N")[..12];

    logger.LogInformation("Received Telegram message {MessageId} from ChatId {ChatId} | {CorrelationId}",
      message.MessageId, message.Chat.Id, correlationId);

    try
    {
      // Отправляем сообщение в Rebus для дальнейшей обработки
      await bus.Send(new ProcessTelegramMessage(
        message.Text ?? string.Empty,
        message.Chat.Id,
        message.MessageId.ToString(),
        correlationId));
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to process Telegram message {MessageId} | {CorrelationId}",
        message.MessageId, correlationId);
    }
  }

  private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
  {
    logger.LogError(exception, "Telegram Bot API error");
    return Task.CompletedTask;
  }
}