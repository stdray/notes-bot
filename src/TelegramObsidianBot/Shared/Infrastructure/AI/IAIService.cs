namespace TelegramObsidianBot.Shared.Infrastructure.AI;

/// <summary>
/// Интерфейс для AI сервисов (суммаризация, генерация тегов)
/// </summary>
public interface IAIService
{
  /// <summary>
  /// Создает краткое содержание контента
  /// </summary>
  Task<string> SummarizeContentAsync(string content, string sourceUrl, CancellationToken cancellationToken = default);

  /// <summary>
  /// Генерирует теги для контента
  /// </summary>
  Task<string[]> GenerateTagsAsync(string content, CancellationToken cancellationToken = default);
}