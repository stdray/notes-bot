namespace TelegramObsidianBot.Shared.Contracts;

// Базовые сообщения для входящих данных
public record TelegramMessageReceived(
  string Text,
  long ChatId,
  string MessageId,
  string CorrelationId);

// Обнаружение ссылок
public record TwitterLinkDetected(
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record YouTubeLinkDetected(
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record ArticleLinkDetected(
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record GitHubLinkDetected(
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

// Извлечение контента
public record TwitterContentExtracted(
  string Content,
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record YouTubeContentExtracted(
  string Title,
  string Description,
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record ArticleContentExtracted(
  string Title,
  string Content,
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record GitHubContentExtracted(
  string RepoName,
  string Description,
  string ReadmeContent,
  string Url,
  long ChatId,
  string MessageId,
  string CorrelationId);

// AI обработка
public record ContentReadyForSummarization(
  string Content,
  string SourceUrl,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record ContentSummarized(
  string Summary,
  string OriginalContent,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record TagsGenerated(
  string[] Tags,
  string Content,
  long ChatId,
  string MessageId,
  string CorrelationId);

// Создание заметок
public record ObsidianNoteReady(
  string Title,
  string Content,
  string[] Tags,
  string SourceUrl,
  long ChatId,
  string MessageId,
  string CorrelationId);

public record ObsidianNoteCreated(
  string FilePath,
  long ChatId,
  string MessageId,
  string CorrelationId);