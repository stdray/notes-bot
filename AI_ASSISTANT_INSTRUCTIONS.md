# Инструкции для AI-ассистента

## Общие правила работы

1. **Следуй архитектурным принципам проекта** - строго соблюдай Screaming Architecture + Vertical Slice Architecture

2. **Используй только разрешенные библиотеки** - Rebus (не MediatR), Mapperly (не AutoMapper), OpenAI

3. **Соблюдай стиль кодирования** - отступы 2 пробела, ширина строки 100 символов, record типы, первичные конструкторы

4. **Обязательная трассировка** - каждое сообщение должно содержать ChatId, MessageId, CorrelationId

5. **Структурированное логирование** - Started/Completed/Failed processing в каждом handler'е

6. **Один слайс = одна ответственность** - минимальные зависимости между слайсами

7. **Self-contained slices** - все необходимое для фичи в одной папке

8. **Добавь в документацию файл prompt_history.md и добавляй в него все то что я напишу в чате**

9. **Сначала делай простые вещи. Используй модули вместо одного файла.**

10. **После каждого ответа пиши свою степень уверенности от 0 до 100%, Если не уверен хотя бы на 80%, задавай мне уточняющие вопросы, прежде чем что-то делать**

11. **Вноси только запрошенные изменения, код комментриуй так чтобы тебе было проще потом с ним работать**

12. **Если надо декомпозируй большие файлы на более мелке, если тебе будет так проще**

13. **Не пиши дублирующийся код - ищи существующие решения в кодовой базе.**

14. **Используй всегда относительные пути и проверяй что они правильные**

15. **Используй context7 для получения актуальной документации библиотек**
    - Всегда используй mcp_context7_resolve-library-id перед получением документации
    - Используй mcp_context7_get-library-docs для получения актуальных примеров кода
    - Предпочитай официальную документацию устаревшим примерам

## Правила использования context7

### Когда использовать context7:
- При работе с новыми или незнакомыми библиотеками
- Когда нужны актуальные примеры кода
- При возникновении ошибок компиляции с внешними библиотеками
- Для проверки правильности использования API

### Процесс работы с context7:
1. **Сначала resolve-library-id** для получения точного ID библиотеки
2. **Затем get-library-docs** с конкретной темой
3. **Применить полученные знания** в коде

### Пример использования:
```
1. mcp_context7_resolve-library-id: "Telegram.Bot"
2. mcp_context7_get-library-docs: "/telegram/telegram.bot" topic: "polling setup"
3. Реализовать код согласно актуальной документации
```

## Структура ответов

### Обязательные элементы в каждом ответе:
- Краткое описание выполненных действий
- Степень уверенности (0-100%)
- Уточняющие вопросы при уверенности < 80%

### Формат степени уверенности:
```
🎯 Степень уверенности: 95%
```

### Формат уточняющих вопросов:
```
❓ Уточняющие вопросы:
1. Вопрос 1?
2. Вопрос 2?
```

## Правила создания кода

### Структура слайса (обязательная):
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

### Обязательные поля в сообщениях:
```csharp
public record SomeCommand(
  // Бизнес-данные
  string SomeData,
  // Обязательные поля трассировки
  long ChatId,
  string MessageId,
  string CorrelationId);
```

### Обязательная структура Handler'а:
```csharp
public class SomeHandler(
  ISomeService service,
  ILogger<SomeHandler> logger) : IHandleMessages<SomeCommand>
{
  public async Task Handle(SomeCommand message)
  {
    // 1. Логирование начала
    logger.LogInformation("Started processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
      nameof(SomeCommand), message.ChatId, message.CorrelationId);

    try
    {
      // 2. Валидация
      // 3. Бизнес-логика
      // 4. Отправка следующего сообщения
      
      // 5. Логирование успеха
      logger.LogInformation("Completed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
        nameof(SomeCommand), message.ChatId, message.CorrelationId);
    }
    catch (Exception ex)
    {
      // 6. Логирование ошибки
      logger.LogError(ex, "Failed processing {MessageType} for ChatId {ChatId} | {CorrelationId}", 
        nameof(SomeCommand), message.ChatId, message.CorrelationId);
      throw;
    }
  }
}
```

## Правила документирования

### Обновление prompt_history.md
- Добавлять все пользовательские запросы
- Указывать дату и время
- Группировать по темам/фичам

### Комментирование кода
- Объяснять бизнес-логику, а не синтаксис
- Указывать связи между компонентами
- Документировать сложные алгоритмы

## Проверочный чек-лист перед ответом

- [ ] Соблюден стиль кодирования (2 пробела, 100 символов)
- [ ] Использованы правильные библиотеки (Rebus, Mapperly)
- [ ] Добавлены обязательные поля трассировки
- [ ] Добавлено логирование Started/Completed/Failed
- [ ] Проверены относительные пути
- [ ] Указана степень уверенности
- [ ] Заданы уточняющие вопросы при уверенности < 80%
- [ ] Обновлен prompt_history.md (если применимо)

---

**Версия:** 1.0  
**Дата создания:** 26 сентября 2025  
**Применимость:** Универсальная для всех проектов