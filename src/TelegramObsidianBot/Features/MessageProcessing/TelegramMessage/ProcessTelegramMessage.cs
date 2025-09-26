namespace TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;

/// <summary>
/// Команда для обработки входящего сообщения из Telegram
/// </summary>
public record ProcessTelegramMessage(
  string Text,
  long ChatId,
  string MessageId,
  string CorrelationId);