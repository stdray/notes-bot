# AI Setup Guide

## Конфигурация AI сервисов

Бот поддерживает два AI провайдера с автоматическим fallback:

### 1. Ollama (Приоритет) - Локальный AI

**Преимущества:**
- Бесплатно
- Работает локально
- Не требует API ключей
- Приватность данных

**Установка:**
```bash
# Скачайте Ollama с https://ollama.ai/
# Установите модель
ollama pull llama3.1

# Проверьте что сервер запущен
curl http://localhost:11434/api/tags
```

**Конфигурация:**
```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.1"
  }
}
```

**Поддерживаемые модели:**
- `llama3.1` (рекомендуется)
- `llama3.2`
- `mistral`
- `codellama`

### 2. OpenRouter (Fallback) - Облачный AI

**Преимущества:**
- Доступ к лучшим моделям
- Высокая надежность
- Не требует локальных ресурсов

**Настройка:**
1. Зарегистрируйтесь на https://openrouter.ai/
2. Получите API ключ
3. Добавьте в конфигурацию

**Конфигурация:**
```json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-your-api-key-here",
    "Model": "anthropic/claude-3-haiku"
  }
}
```

**Рекомендуемые модели:**
- `anthropic/claude-3-haiku` - быстро, дешево
- `openai/gpt-4o-mini` - хорошее качество
- `meta-llama/llama-3.1-8b-instruct` - бесплатно

## Логика Fallback

1. **Сначала Ollama:** Бот пытается использовать локальный Ollama
2. **При ошибке OpenRouter:** Автоматически переключается на облачный AI
3. **При ошибке AI:** Использует базовые алгоритмы для тегов

## Мониторинг AI

Все AI операции логируются:

```
[12:34:56 INF] Attempting summarization with Ollama
[12:34:57 INF] Successfully completed summarization with Ollama
[12:34:58 INF] Attempting tag generation with Ollama  
[12:34:59 WRN] Ollama failed for tag generation, falling back to OpenRouter
[12:35:01 INF] Successfully completed tag generation with OpenRouter
```

## Производительность

### Ollama (локально)
- Суммаризация: ~5-15 секунд
- Генерация тегов: ~3-8 секунд
- Зависит от мощности компьютера

### OpenRouter (облачно)
- Суммаризация: ~2-5 секунд
- Генерация тегов: ~1-3 секунды
- Зависит от выбранной модели

## Troubleshooting

### Ollama не отвечает
```bash
# Проверьте статус
ollama list

# Перезапустите сервис
ollama serve

# Проверьте логи
tail -f ~/.ollama/logs/server.log
```

### OpenRouter ошибки
- Проверьте API ключ
- Проверьте баланс аккаунта
- Убедитесь что модель доступна

### Полный fallback на базовые теги
Если оба AI провайдера недоступны, бот использует простые правила:
- `video` - для YouTube контента
- `article` - для статей
- `development` - для GitHub
- `social-media` - для Twitter
- `telegram-bot`, `auto-generated` - всегда добавляются

## Стоимость

### Ollama
- **Бесплатно** - только электричество

### OpenRouter
- Claude 3 Haiku: ~$0.25 за 1M токенов
- GPT-4o Mini: ~$0.15 за 1M токенов
- Llama 3.1 8B: **Бесплатно** (лимиты)

**Примерная стоимость на 1000 заметок:**
- Суммаризация: $0.10-0.50
- Генерация тегов: $0.05-0.25
- **Итого: ~$0.15-0.75**