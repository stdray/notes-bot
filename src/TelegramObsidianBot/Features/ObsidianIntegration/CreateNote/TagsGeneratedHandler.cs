using Rebus.Handlers;
using Rebus.Bus;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Features.ObsidianIntegration.CreateNote;

/// <summary>
/// Обработчик сгенерированных тегов
/// Подготавливает заметку для создания в Obsidian
/// </summary>
public class TagsGeneratedHandler(
  IBus bus,
  ILogger<TagsGeneratedHandler> logger) : IHandleMessages<TagsGenerated>
{
  public async Task Handle(TagsGenerated message)
  {
    // Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
      nameof(TagsGenerated), message.Meta.ChatId, message.Meta.CorrelationId);

    try
    {
      // Генерируем заголовок заметки
      var title = GenerateNoteTitle(message.Content);

      logger.LogInformation("Prepared Obsidian note '{Title}' with {TagCount} tags | {CorrelationId}",
        title, message.Tags.Length, message.Meta.CorrelationId);

      // Отправляем готовую заметку для создания
      await bus.Send(new ObsidianNoteReady(
        title,
        message.Content,
        message.Tags,
        ExtractSourceUrl(message.Content),
        message.Meta));

      // Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TagsGenerated), message.Meta.ChatId, message.Meta.CorrelationId);
    }
    catch (Exception ex)
    {
      // Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}",
        nameof(TagsGenerated), message.Meta.ChatId, message.Meta.CorrelationId);
      throw;
    }
  }

  private static string GenerateNoteTitle(string content)
  {
    // Простая генерация заголовка из первых слов контента
    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
    var firstLine = lines.FirstOrDefault() ?? "Untitled Note";
    
    // Убираем префиксы и ограничиваем длину
    var title = firstLine
      .Replace("Summary of content from", "")
      .Replace("Title:", "")
      .Trim()
      .Split(' ')
      .Take(6)
      .Aggregate((a, b) => $"{a} {b}")
      .Trim();

    return string.IsNullOrEmpty(title) ? "Auto-generated Note" : title;
  }

  private static string ExtractSourceUrl(string content)
  {
    // Простое извлечение URL из контента
    var lines = content.Split('\n');
    foreach (var line in lines)
    {
      if (line.Contains("http"))
      {
        var words = line.Split(' ');
        var url = words.FirstOrDefault(w => w.StartsWith("http"));
        if (url != null) return url.TrimEnd(':');
      }
    }
    return string.Empty;
  }
}