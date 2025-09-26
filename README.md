# Telegram Obsidian Bot

Telegram-бот для автоматического создания заметок в Obsidian на основе сообщений пользователя.

## Архитектура

- **Screaming Architecture + Vertical Slice Architecture**
- **Event-driven через Rebus**
- **.NET 9**
- **Стиль кодирования:** 2 пробела, 100 символов, record типы, первичные конструкторы

## Структура проекта

```
TelegramObsidianBot/
├── src/
│   └── TelegramObsidianBot/
│       ├── Features/                    # Vertical Slices
│       │   ├── MessageProcessing/
│       │   ├── ContentExtraction/
│       │   ├── NoteGeneration/
│       │   └── ObsidianIntegration/
│       ├── Shared/
│       │   ├── Infrastructure/          # Rebus, конфигурация
│       │   ├── Common/                  # Общие модели
│       │   └── Contracts/               # Сообщения Rebus
│       └── Program.cs
├── tests/
└── docs/
```

## Технологический стек

### Основные библиотеки
- **Rebus** - service bus для межслайсовых коммуникаций
- **Riok.Mapperly** - source generator для маппинга
- **OpenAI** - AI интеграция (Ollama + OpenRouter fallback)
- **Telegram.Bot** - Telegram Bot API
- **FluentValidation** - валидация
- **Serilog** - структурированное логирование

### Трассировка
Каждое сообщение содержит:
- `ChatId` - ID чата в Telegram
- `MessageId` - ID сообщения в Telegram  
- `CorrelationId` - для трассировки всего pipeline

## Конфигурация

Скопируйте `appsettings.json` и настройте:

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
    "VaultPath": "path-to-your-vault",
    "DefaultFolder": "Inbox"
  }
}
```

## Запуск

```bash
# Сборка
dotnet build

# Запуск
dotnet run --project src/TelegramObsidianBot

# Тесты
dotnet test
```

## Разработка

### Создание нового слайса

1. Создайте папку в `Features/`
2. Добавьте файлы согласно структуре:
   - `{Feature}Command.cs` - Rebus сообщение
   - `{Feature}Handler.cs` - обработчик
   - `{Feature}Service.cs` - бизнес-логика
   - `{Feature}Mapper.cs` - Mapperly mapper
   - `{Feature}Validator.cs` - валидация
   - `{Feature}ServiceExtensions.cs` - регистрация DI

### Логирование

Обязательно в каждом handler'е:
```csharp
// Начало
logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
  nameof(Command), message.ChatId, message.CorrelationId);

// Успех
logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
  nameof(Command), message.ChatId, message.CorrelationId);

// Ошибка
logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
  nameof(Command), message.ChatId, message.CorrelationId);
```

## Статус

🚧 **В разработке**

### Готово
- [x] Базовая структура проекта (.NET 9)
- [x] Конфигурация Rebus + Serilog
- [x] Базовые контракты сообщений
- [x] **Telegram Bot интеграция (polling)**
- [x] **Определение типов ссылок (YouTube, Twitter, GitHub, статьи)**
- [x] **Базовый pipeline обработки YouTube ссылок**
- [x] **AI интеграция (Ollama + OpenRouter fallback)**
- [x] **Реальная суммаризация контента через AI**
- [x] **AI генерация тегов с fallback на базовые теги**
- [x] **Twitter/X интеграция с поддержкой прокси (LinqToTwitter)**
- [x] **Рекурсивная обработка ссылок из твитов**
- [x] **Создание заметок в Obsidian (файловая система)**
- [x] **Unit тесты (13+ тестов проходят)**

### Реализованный pipeline
```
Telegram Message → Link Detection → Content Extraction (YouTube/Twitter/Articles) → AI Processing → Obsidian Note
```

### AI возможности
- **Ollama (приоритет):** Локальный AI без API ключей
- **OpenRouter (fallback):** Облачный AI при недоступности Ollama
- **Суммаризация:** Создание кратких содержаний до 300 слов
- **Генерация тегов:** 3-7 релевантных тегов с AI + базовые теги
- **Graceful degradation:** При ошибке AI используются базовые теги

### Twitter/X возможности
- **Извлечение контента твитов** через LinqToTwitter API
- **Поддержка прокси** для обхода ограничений
- **Рекурсивная обработка** ссылок внутри твитов
- **Graceful degradation** при недоступности API
- **Валидация URL** и обработка ошибок

---

**Версия:** 0.1.0  
**Дата:** 26 сентября 2025