# Правила реализации проекта

## Общие принципы

### Стиль кодирования
1. **Форматирование кода**
   - Отступы: **2 пробела** (не табы)
   - Максимальная ширина строки: **100 символов**
   - Перенос длинных строк с правильным выравниванием

2. **Современные возможности C#**
   - Использовать **record** типы везде, где возможно
   - Использовать **первичные конструкторы** (Primary Constructors)
   - Предпочитать file-scoped namespaces
   - Использовать global using для часто используемых пространств имен

3. **Примеры стиля**
   ```csharp
   // Record с первичным конструктором
   public record ExtractYouTubeContent(
     string Url,
     long ChatId,
     string MessageId);

   // Класс с первичным конструктором
   public class YouTubeContentHandler(
     IYouTubeApiService youTubeService,
     ILogger<YouTubeContentHandler> logger) : IHandleMessages<ExtractYouTubeContent>
   {
     public async Task Handle(ExtractYouTubeContent message)
     {
       logger.LogInformation("Processing YouTube content extraction for {Url}", 
         message.Url);
       // ...
     }
   }

   // Длинные строки с переносом
   services.AddSingleton<OpenAIClient>("ollama", provider =>
     new OpenAIClient(
       apiKey: "not-needed",
       new OpenAIClientOptions
       {
         Endpoint = new Uri("http://localhost:11434/v1")
       }));
   ```

### Архитектурные требования
1. **Строго следовать Screaming Architecture + Vertical Slice Architecture**
   - Каждая фича в отдельной папке
   - Никаких технических слоев (Controllers, Services, Repositories)
   - Структура должна кричать о бизнес-логике

2. **Один слайс = одна ответственность**
   - Каждый слайс решает одну конкретную задачу
   - Минимальные зависимости между слайсами
   - Взаимодействие только через Rebus сообщения

3. **Self-contained slices**
   - Все необходимое для фичи находится в одной папке
   - Собственные модели, валидация, маппинг
   - Собственные тесты

### Технологические ограничения

#### Обязательные библиотеки
- **Rebus** (НЕ MediatR) - для всех межслайсовых коммуникаций
- **Riok.Mapperly** (НЕ AutoMapper) - для всего маппинга
- **OpenAI** библиотека - для всех AI интеграций
- **Rebus.InMemory** - для транспорта сообщений

#### Запрещенные библиотеки
- ❌ MediatR
- ❌ AutoMapper
- ❌ Любые платные библиотеки
- ❌ Entity Framework (если не требуется явно)

## Структура кода

### Именование файлов и классов

#### Сообщения (Messages)
```csharp
// Формат: {Action}{Entity}.cs
ExtractYouTubeContent.cs
TwitterLinkDetected.cs
ContentSummarized.cs
```

#### Обработчики (Handlers)
```csharp
// Формат: {MessageName}Handler.cs
ExtractYouTubeContentHandler.cs
TwitterLinkDetectedHandler.cs
ContentSummarizedHandler.cs
```

#### Сервисы
```csharp
// Формат: {Entity}{Purpose}Service.cs
YouTubeApiService.cs
ContentExtractionService.cs
TagGenerationService.cs
```

#### Мапперы (Mapperly)
```csharp
// Формат: {Entity}Mapper.cs
YouTubeContentMapper.cs
TwitterContentMapper.cs
```

### Структура слайса (обязательная)

```
FeatureName/
├── {FeatureName}Command.cs          // Rebus message
├── {FeatureName}Handler.cs          // IHandleMessages<T>
├── {FeatureName}Service.cs          // Core business logic
├── {FeatureName}Mapper.cs           // Mapperly mapper
├── {FeatureName}Validator.cs        // FluentValidation
├── {FeatureName}ServiceExtensions.cs // DI registration
└── {FeatureName}Tests.cs            // Unit tests
```

### Регистрация сервисов

#### Каждый слайс должен иметь extension method
```csharp
// Обязательный файл: {FeatureName}ServiceExtensions.cs
public static class YouTubeContentServiceExtensions
{
  public static IServiceCollection AddYouTubeContentFeature(
    this IServiceCollection services)
  {
    // Регистрация всех сервисов слайса
    services.AddScoped<IYouTubeApiService, YouTubeApiService>();
    services.AddScoped<YouTubeContentMapper>();
    services.AddScoped<YouTubeContentValidator>();
    
    return services;
  }
}
```

## Rebus требования

### Сообщения (Messages)
1. **Только record типы с первичными конструкторами**
   ```csharp
   public record ExtractYouTubeContent(
     string Url,
     long ChatId,
     string MessageId);
   ```

2. **Обязательные поля в каждом сообщении:**
   - `ChatId` (long) - для связи с пользователем
   - `MessageId` (string) - для трассировки
   - `CorrelationId` (string) - для связи всех сообщений в pipeline

3. **Трассировка pipeline:**
   - `CorrelationId` генерируется при получении первого Telegram сообщения
   - Передается во всех последующих сообщениях pipeline
   - Используется в логах для связи всех этапов обработки
   - Формат: `{ChatId}_{MessageId}_{Timestamp}` или GUID

4. **Пример трассировки:**
   ```csharp
   public record TelegramMessageReceived(
     string Text,
     long ChatId,
     string MessageId,
     string CorrelationId); // Генерируется здесь

   public record TwitterLinkDetected(
     string Url,
     long ChatId,
     string MessageId,
     string CorrelationId); // Передается дальше

   public record TwitterContentExtracted(
     string Content,
     long ChatId,
     string MessageId,
     string CorrelationId); // И так далее...
   ```

5. **Именование сообщений:**
   - Команды: `{Verb}{Noun}` (ExtractContent, CreateNote)
   - События: `{Noun}{PastTense}` (ContentExtracted, NoteCreated)

### Обработчики (Handlers)
1. **Один handler = одно сообщение с первичным конструктором**
   ```csharp
   public class ExtractYouTubeContentHandler(
     IYouTubeApiService youTubeService,
     ILogger<ExtractYouTubeContentHandler> logger) : IHandleMessages<ExtractYouTubeContent>
   {
     public async Task Handle(ExtractYouTubeContent message)
     {
       logger.LogInformation("Processing YouTube extraction for ChatId {ChatId}", 
         message.ChatId);
       // Обработка
     }
   }
   ```

2. **Обязательная структура handler'а:**
   - **Логирование начала обработки** (Started processing)
   - Валидация входящих данных
   - Бизнес-логика
   - Отправка следующего сообщения
   - **Логирование успешного завершения** (Completed processing)
   - **Обработка ошибок с логированием** (Failed processing)

## Mapperly требования

### Объявление мапперов
```csharp
[Mapper]
public partial class YouTubeContentMapper
{
  public partial ExtractedContent MapToExtractedContent(YouTubeVideo video);
  
  [MapProperty(nameof(YouTubeVideo.Snippet.Title), 
    nameof(ExtractedContent.Title))]
  public partial ExtractedContent MapYouTubeVideo(YouTubeVideo video);
}
```

### Правила маппинга
1. **Partial классы обязательны**
2. **Explicit маппинг для сложных случаев**
3. **Один mapper на слайс**
4. **Регистрация в DI как Scoped**

## AI интеграция требования

### Конфигурация клиентов
```csharp
// Ollama клиент (приоритет)
services.AddSingleton<OpenAIClient>("ollama", provider =>
  new OpenAIClient(
    apiKey: "not-needed",
    new OpenAIClientOptions
    {
      Endpoint = new Uri("http://localhost:11434/v1")
    }));

// OpenRouter клиент (fallback)
services.AddSingleton<OpenAIClient>(provider =>
  new OpenAIClient(
    apiKey: configuration["OpenRouter:ApiKey"],
    new OpenAIClientOptions
    {
      Endpoint = new Uri("https://openrouter.ai/api/v1")
    }));
```

### Fallback логика (обязательная)
1. **Всегда сначала Ollama**
2. **При ошибке - OpenRouter**
3. **Логирование переключений**
4. **Конфигурируемые модели**

## Тестирование требования

### Unit тесты
1. **Каждый слайс должен иметь тесты**
2. **Тестирование handler'ов с mock Rebus**
3. **Тестирование mapper'ов**
4. **Тестирование валидаторов**

### Именование тестов
```csharp
// Формат: {MethodName}_When{Condition}_Should{ExpectedResult}
[Test]
public async Task Handle_WhenValidYouTubeUrl_ShouldExtractContent()
{
  // Arrange
  var message = new ExtractYouTubeContent(
    "https://youtube.com/watch?v=test",
    12345,
    "msg_123");
  
  // Act  
  await handler.Handle(message);
  
  // Assert
  // ...
}
```

## Обработка ошибок

### Обязательные try-catch блоки
1. **В каждом handler'е**
2. **При вызовах внешних API**
3. **При AI запросах (с fallback)**

### Логирование
```csharp
// Обязательная структура логов с CorrelationId
logger.LogInformation("Processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
  nameof(ExtractYouTubeContent), message.ChatId, message.CorrelationId);

logger.LogError(ex, "Failed to extract YouTube content for {Url} | {CorrelationId}", 
  message.Url, message.CorrelationId);

// Начало pipeline
logger.LogInformation("Starting pipeline for Telegram message | {CorrelationId}", 
  correlationId);

// Завершение pipeline
logger.LogInformation("Pipeline completed successfully | {CorrelationId}", 
  correlationId);
```

### Обязательное логирование в Handler'ах
```csharp
public class ExtractYouTubeContentHandler(
  IYouTubeApiService youTubeService,
  ILogger<ExtractYouTubeContentHandler> logger) : IHandleMessages<ExtractYouTubeContent>
{
  public async Task Handle(ExtractYouTubeContent message)
  {
    // ОБЯЗАТЕЛЬНО: Логирование начала обработки
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
      nameof(ExtractYouTubeContent), message.ChatId, message.CorrelationId);

    try
    {
      // Бизнес-логика
      var content = await youTubeService.ExtractAsync(message.Url);
      
      // ОБЯЗАТЕЛЬНО: Логирование успешного завершения
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
        nameof(ExtractYouTubeContent), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // ОБЯЗАТЕЛЬНО: Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
        nameof(ExtractYouTubeContent), message.ChatId, message.CorrelationId);
      throw;
    }
  }
}
```

### Structured Logging требования
1. **Обязательные поля в каждом логе:**
   - `CorrelationId` - для трассировки pipeline
   - `ChatId` - для связи с пользователем
   - `MessageType` - тип обрабатываемого сообщения
   - `Timestamp` - автоматически добавляется Serilog

2. **Обязательные логи в каждом Handler'е:**
   - **Started processing** - в начале метода Handle
   - **Completed processing** - при успешном завершении
   - **Failed processing** - при ошибке (в catch блоке)

3. **Формат CorrelationId:**
   ```csharp
   // При создании первого сообщения
   var correlationId = $"{chatId}_{messageId}_{DateTimeOffset.UtcNow.Ticks}";
   
   // Или использовать GUID для краткости
   var correlationId = Guid.NewGuid().ToString("N")[..12]; // 12 символов
   ```

4. **Пример полного лога pipeline:**
   ```
   [12:34:56.123] INFO: Started processing TelegramMessageReceived for ChatId 12345 | abc123def456
   [12:34:56.145] INFO: Completed processing TelegramMessageReceived for ChatId 12345 | abc123def456
   [12:34:56.150] INFO: Started processing TwitterLinkDetected for ChatId 12345 | abc123def456
   [12:34:57.234] INFO: Completed processing TwitterLinkDetected for ChatId 12345 | abc123def456
   [12:34:57.240] INFO: Started processing YouTubeContentExtracted for ChatId 12345 | abc123def456
   [12:35:02.567] INFO: Completed processing YouTubeContentExtracted for ChatId 12345 | abc123def456
   ```

## Конфигурация

### appsettings.json структура
```json
{
  "Telegram": {
    "BotToken": "your-bot-token"
  },
  "OpenRouter": {
    "ApiKey": "your-openrouter-key",
    "Model": "anthropic/claude-3-haiku"
  },
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.1"
  },
  "Obsidian": {
    "VaultPath": "path-to-vault",
    "DefaultFolder": "Inbox"
  },
  "YouTube": {
    "ApiKey": "your-youtube-key"
  }
}
```

## Запрещенные практики

### ❌ НЕ делать
1. **Прямые вызовы между слайсами** - только через Rebus
2. **Shared модели между слайсами** - каждый слайс свои модели
3. **Технические слои** (Controllers, Services, Repositories)
4. **Анемичные модели** - логика должна быть в доменных объектах
5. **Большие handler'ы** - разбивать на методы
6. **Синхронные вызовы AI** - всегда async/await

### ✅ Обязательно делать
1. **Валидация всех входящих данных**
2. **Логирование всех операций**
3. **Graceful degradation при ошибках AI**
4. **Retry policies для внешних API**
5. **Тесты для каждого слайса**

## Порядок реализации

### Этап 1: Инфраструктура
1. Настройка проекта и зависимостей
2. Конфигурация Rebus
3. Базовые сообщения и контракты

### Этап 2: Базовые слайсы
1. TelegramMessageProcessing
2. LinkDetection
3. ObsidianIntegration

### Этап 3: Content Extraction
1. YouTubeContent
2. ArticleContent  
3. TwitterContent
4. GitHubContent

### Этап 4: AI Features
1. ContentSummarization
2. TagGeneration
3. NoteGeneration

### Этап 5: Интеграция и тестирование
1. End-to-end pipeline
2. Error handling
3. Performance optimization

## Критерии готовности

### Definition of Done для слайса
- ✅ Реализованы все компоненты (Handler, Service, Mapper, Validator)
- ✅ Написаны unit тесты
- ✅ Добавлена регистрация в DI
- ✅ Добавлено логирование
- ✅ Обработка ошибок
- ✅ Документация в коде

### Definition of Done для проекта
- ✅ Все слайсы реализованы
- ✅ Pipeline работает end-to-end
- ✅ Конфигурация вынесена в appsettings
- ✅ Тесты покрывают основные сценарии
- ✅ README с инструкциями по запуску

---

**Важно:** Эти правила обязательны для выполнения. Любое отклонение должно быть явно обосновано и согласовано.