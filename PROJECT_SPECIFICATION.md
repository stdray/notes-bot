# Telegram Obsidian Bot - Спецификация проекта

## Описание проекта

Telegram-бот для автоматического создания заметок в Obsidian на основе сообщений пользователя. Бот анализирует ссылки, извлекает контент, создает краткие содержания и автоматически подбирает теги для заметок.

## Архитектура

### Основной подход
- **Screaming Architecture** - структура проекта отражает бизнес-логику
- **Vertical Slice Architecture** - организация по фичам, а не по техническим слоям
- **Event-driven architecture** - взаимодействие через сообщения

### Структура проекта

```
TelegramObsidianBot/
├── src/
│   ├── Features/
│   │   ├── MessageProcessing/
│   │   │   ├── TextMessage/
│   │   │   ├── LinkMessage/
│   │   │   └── MediaMessage/
│   │   ├── ContentExtraction/
│   │   │   ├── YouTubeContent/
│   │   │   ├── ArticleContent/
│   │   │   ├── TwitterContent/
│   │   │   └── GitHubContent/
│   │   ├── NoteGeneration/
│   │   │   ├── CreateNote/
│   │   │   ├── TagSuggestion/
│   │   │   └── ContentSummarization/
│   │   ├── ObsidianIntegration/
│   │   │   ├── CreateFile/
│   │   │   ├── UpdateTags/
│   │   │   └── ManageVault/
│   │   └── UserManagement/
│   │       ├── Registration/
│   │       ├── Settings/
│   │       └── Preferences/
│   ├── Shared/
│   │   ├── Infrastructure/
│   │   ├── Common/
│   │   └── Contracts/
│   └── Host/
│       └── Program.cs
├── tests/
└── docs/
```

### Vertical Slice структура (пример)

```
YouTubeContent/
├── ExtractYouTubeContent.cs          // Command (Rebus message)
├── ExtractYouTubeContentHandler.cs   // Handler (IHandleMessages<T>)
├── YouTubeContentExtractor.cs        // Core logic
├── YouTubeApiClient.cs               // External API
├── YouTubeContentValidator.cs        // Validation
├── YouTubeContentMapper.cs           // Mapperly mapper
└── YouTubeContentTests.cs            // Tests
```

## Технологический стек

### Основные библиотеки

#### Messaging (замена MediatR)
- `Rebus` - основная библиотека service bus
- `Rebus.ServiceProvider` - интеграция с DI
- `Rebus.InMemory` - in-memory транспорт для сообщений

#### Маппинг
- `Riok.Mapperly` - source generator для маппинга, zero-runtime overhead

#### AI/LLM
- `OpenAI` - официальная библиотека OpenAI (поддерживает OpenRouter и Ollama через совместимые endpoints)

#### Telegram Bot
- `Telegram.Bot` - основная библиотека для работы с Telegram Bot API
- `Telegram.Bot.Extensions.Polling` - для обработки обновлений

#### Web Framework
- `ASP.NET Core` - для создания веб-сервиса
- `Microsoft.Extensions.Hosting` - для фонового сервиса

#### Валидация и HTTP
- `FluentValidation` - валидация в каждом слайсе
- `HttpClientFactory` - для HTTP запросов
- `Polly` - для retry policies и circuit breaker

#### Парсинг и обработка контента
- `AngleSharp` - для парсинга HTML
- `YamlDotNet` - для работы с YAML (frontmatter в Obsidian)
- `Markdig` - для работы с Markdown

#### Внешние API
- `Google.Apis.YouTube.v3` - YouTube Data API
- `linqtotwitter` - Twitter API
- `HtmlAgilityPack` - дополнительный HTML парсер

### AI провайдеры

#### Конфигурация AI сервисов
- **Ollama** - локальный AI (бесплатно, приоритет)
- **OpenRouter** - облачный AI (fallback)
- Единый интерфейс через библиотеку OpenAI

#### Fallback логика
1. Сначала пробуем Ollama (локально, бесплатно)
2. При ошибке переключаемся на OpenRouter
3. Возможность настройки разных моделей для разных задач

## Функциональность

### Поддерживаемые типы контента

#### YouTube
- Извлечение метаданных видео
- Получение описания и заголовка
- Создание краткого содержания

#### Статьи (веб-страницы)
- Парсинг HTML контента
- Извлечение основного текста
- Создание краткого содержания

#### Twitter/X
- Извлечение текста твита
- Поиск ссылок внутри твитов
- Обработка медиа-контента
- Рекурсивная обработка найденных ссылок

#### GitHub
- Извлечение README
- Описание репозитория
- Основная информация о проекте

#### Habr и другие статьи
- Специализированные парсеры для популярных ресурсов
- Универсальный парсер для остальных сайтов

### Обработка заметок

#### Автоматическое тегирование
- Анализ контента с помощью AI
- Подбор релевантных тегов из существующих
- Предложение новых тегов

#### Генерация заметок
- Создание структурированных заметок в Markdown
- Добавление метаданных (frontmatter)
- Сохранение источников и контекста

#### Интеграция с Obsidian
- Создание файлов в указанной папке
- Управление тегами
- Поддержка шаблонов заметок

## Pipeline обработки

### Пример: Twitter с YouTube ссылкой

```
TelegramMessageReceived (CorrelationId: abc123def456)
         ↓
TwitterLinkDetected (CorrelationId: abc123def456)
         ↓
TwitterContentExtracted (CorrelationId: abc123def456)
         ↓
YouTubeLinkFoundInTweet (CorrelationId: abc123def456)
         ↓
YouTubeContentExtracted (CorrelationId: abc123def456)
         ↓
ContentReadyForSummarization (CorrelationId: abc123def456)
         ↓
ContentSummarized (CorrelationId: abc123def456)
         ↓
TagsGenerated (CorrelationId: abc123def456)
         ↓
ObsidianNoteReady (CorrelationId: abc123def456)
```

### Трассировка в логах

```
[12:34:56] INFO: Starting pipeline for Telegram message | abc123def456
[12:34:56] INFO: Processing TelegramMessageReceived for ChatId 12345 | abc123def456
[12:34:57] INFO: Processing TwitterLinkDetected for ChatId 12345 | abc123def456
[12:34:58] INFO: Processing TwitterContentExtracted for ChatId 12345 | abc123def456
[12:34:59] INFO: Processing YouTubeLinkFoundInTweet for ChatId 12345 | abc123def456
[12:35:02] INFO: Processing YouTubeContentExtracted for ChatId 12345 | abc123def456
[12:35:05] INFO: Processing ContentSummarized for ChatId 12345 | abc123def456
[12:35:06] INFO: Processing TagsGenerated for ChatId 12345 | abc123def456
[12:35:07] INFO: Pipeline completed successfully | abc123def456
```

### Основные сообщения (Messages)

```csharp
// Входящие сообщения (все содержат ChatId, MessageId, CorrelationId)
public record TelegramMessageReceived(
  string Text, long ChatId, string MessageId, string CorrelationId);

public record TwitterLinkDetected(
  string Url, long ChatId, string MessageId, string CorrelationId);

public record YouTubeLinkDetected(
  string Url, long ChatId, string MessageId, string CorrelationId);

public record ArticleLinkDetected(
  string Url, long ChatId, string MessageId, string CorrelationId);

public record GitHubLinkDetected(
  string Url, long ChatId, string MessageId, string CorrelationId);

// Обработка контента
public record TwitterContentExtracted(
  string Content, string Url, long ChatId, string MessageId, string CorrelationId);

public record YouTubeContentExtracted(
  string Title, string Description, string Url, long ChatId, string MessageId, 
  string CorrelationId);

public record ArticleContentExtracted(
  string Title, string Content, string Url, long ChatId, string MessageId, 
  string CorrelationId);

public record GitHubContentExtracted(
  string RepoName, string Description, string ReadmeContent, string Url, 
  long ChatId, string MessageId, string CorrelationId);

// AI обработка
public record ContentReadyForSummarization(
  string Content, string SourceUrl, long ChatId, string MessageId, 
  string CorrelationId);

public record ContentSummarized(
  string Summary, string OriginalContent, long ChatId, string MessageId, 
  string CorrelationId);

public record TagsGenerated(
  string[] Tags, string Content, long ChatId, string MessageId, 
  string CorrelationId);

// Создание заметок
public record ObsidianNoteReady(
  string Title, string Content, string[] Tags, string SourceUrl, 
  long ChatId, string MessageId, string CorrelationId);

public record ObsidianNoteCreated(
  string FilePath, long ChatId, string MessageId, string CorrelationId);
```

## Паттерны проектирования

### Основные паттерны
- **Request/Response Pattern** - четкие контракты для каждой фичи
- **Handler Pattern** - один обработчик на одну фичу
- **Strategy Pattern** - для различных типов обработчиков контента
- **Factory Pattern** - для создания обработчиков по типу контента

### Архитектурные паттерны
- **Self-contained Slice** - каждый слайс содержит всю необходимую логику
- **Event-driven communication** - слайсы общаются через события
- **Anti-corruption Layer** - изоляция внешних зависимостей

## Конфигурация

### Настройки приложения
- Telegram Bot Token
- OpenRouter API Key
- Ollama endpoint
- Obsidian vault path
- YouTube API Key
- Twitter API credentials

### AI модели
- Ollama: llama3.1 (или другая локальная модель)
- OpenRouter: anthropic/claude-3-haiku (или другая модель)
- Возможность настройки разных моделей для суммаризации и тегирования

## Безопасность и производительность

### Безопасность
- Хранение API ключей в конфигурации
- Валидация входящих данных
- Rate limiting для внешних API

### Производительность
- Кэширование результатов AI обработки
- Retry policies для внешних сервисов
- Асинхронная обработка через Rebus

### Мониторинг
- Структурированное логирование (Serilog)
- Метрики производительности
- Отслеживание прохождения сообщений через pipeline

## Тестирование

### Типы тестов
- Unit тесты для каждого слайса
- Integration тесты для pipeline
- End-to-end тесты для основных сценариев

### Тестовая инфраструктура
- In-memory Rebus для тестов
- Mock внешних API
- Тестовые данные для AI сервисов

## Развертывание

### Требования
- .NET 8.0+
- Ollama (опционально, для локального AI)
- SQL Server или PostgreSQL (для продакшена с Rebus)

### Конфигурация окружений
- Development: Rebus.InMemory
- Production: Rebus.SqlServer или Rebus.PostgreSQL

## Расширяемость

### Добавление новых типов контента
1. Создать новый слайс в ContentExtraction
2. Добавить соответствующие сообщения
3. Реализовать handler для обработки
4. Зарегистрировать сервисы

### Добавление новых AI провайдеров
1. Реализовать интерфейс через OpenAI библиотеку
2. Добавить в fallback цепочку
3. Настроить конфигурацию

### Интеграция с другими системами заметок
1. Создать новый слайс в интеграциях
2. Реализовать соответствующие интерфейсы
3. Добавить конфигурацию

## Версионирование

- Семантическое версионирование (SemVer)
- Обратная совместимость API
- Миграции конфигурации при необходимости

---

**Дата создания:** 26 сентября 2025  
**Версия документа:** 1.0  
**Статус:** Утверждено для реализации