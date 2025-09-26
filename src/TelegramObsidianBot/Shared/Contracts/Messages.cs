namespace TelegramObsidianBot.Shared.Contracts;


// Общие метаданные для сообщений
public record MessageMeta(
    long ChatId,
    string MessageId,
    string CorrelationId);

// Базовые сообщения для входящих данных
public record TelegramMessageReceived(
    string Text,
    MessageMeta Meta);

// Обнаружение ссылок
public record TwitterLinkDetected(
    string Url,
    MessageMeta Meta);

public record YouTubeLinkDetected(
    string Url,
    MessageMeta Meta);

public record ArticleLinkDetected(
    string Url,
    MessageMeta Meta);

public record GitHubLinkDetected(
    string Url,
    MessageMeta Meta);

// Извлечение контента
public record TwitterContentExtracted(
    string Content,
    string Url,
    MessageMeta Meta);

public record YouTubeContentExtracted(
    string Title,
    string Description,
    string Url,
    MessageMeta Meta);

public record ArticleContentExtracted(
    string Title,
    string Content,
    string Url,
    MessageMeta Meta);

public record GitHubContentExtracted(
    string RepoName,
    string Description,
    string ReadmeContent,
    string Url,
    MessageMeta Meta);

// AI обработка
public record ContentReadyForSummarization(
    string Content,
    string SourceUrl,
    MessageMeta Meta);

public record ContentSummarized(
    string Summary,
    string OriginalContent,
    MessageMeta Meta);

public record TagsGenerated(
    string[] Tags,
    string Content,
    MessageMeta Meta);

// Создание заметок
public record ObsidianNoteReady(
    string Title,
    string Content,
    string[] Tags,
    string SourceUrl,
    MessageMeta Meta);

public record ObsidianNoteCreated(
    string FilePath,
    MessageMeta Meta);