# Twitter/X API Setup Guide

## Получение Twitter API ключей

### 1. Создание Twitter Developer Account

1. Перейдите на https://developer.twitter.com/
2. Войдите в свой Twitter аккаунт
3. Подайте заявку на Developer Account
4. Дождитесь одобрения (может занять несколько дней)

### 2. Создание приложения

1. В Developer Portal создайте новое приложение
2. Заполните информацию о приложении:
   - **App name:** TelegramObsidianBot
   - **Description:** Bot for creating Obsidian notes from Telegram messages
   - **Website:** https://github.com/your-username/telegram-obsidian-bot

### 3. Получение ключей

В настройках приложения получите:
- **Consumer Key (API Key)**
- **Consumer Secret (API Secret)**
- **Access Token**
- **Access Token Secret**

## Конфигурация

### appsettings.Development.json
```json
{
  "Twitter": {
    "ConsumerKey": "your-consumer-key-here",
    "ConsumerSecret": "your-consumer-secret-here", 
    "AccessToken": "your-access-token-here",
    "AccessTokenSecret": "your-access-token-secret-here"
  }
}
```

## Поддержка прокси

### Когда нужен прокси
- Ограничения доступа к Twitter API в вашем регионе
- Корпоративные сети с ограничениями
- Необходимость скрыть реальный IP

### Настройка прокси
```json
{
  "Proxy": {
    "Host": "proxy.example.com",
    "Port": 8080,
    "Username": "proxy-username",
    "Password": "proxy-password"
  }
}
```

### Отключение прокси
Оставьте поля пустыми для работы без прокси:
```json
{
  "Proxy": {
    "Host": "",
    "Port": 0,
    "Username": "",
    "Password": ""
  }
}
```

## Функциональность

### Что извлекается из твитов
- **Текст твита**
- **Автор** (имя и username)
- **Дата создания**
- **Встроенные ссылки** (для рекурсивной обработки)
- **Медиа URL** (изображения, видео)

### Рекурсивная обработка
Если в твите найдены ссылки на:
- **YouTube** → извлекается контент видео
- **GitHub** → извлекается информация о репозитории  
- **Статьи** → извлекается текст статьи

### Пример pipeline
```
Twitter URL → Tweet Content → Embedded YouTube URL → Video Content → AI Summary → Obsidian Note
```

## Ограничения API

### Twitter API v1.1 (LinqToTwitter)
- **Rate Limits:** 300 запросов на 15 минут
- **Аутентификация:** OAuth 1.0a
- **Доступ:** Требует одобренный Developer Account

### Обработка ошибок
- **Rate Limit:** Автоматическая задержка
- **Недоступный твит:** Логирование и пропуск
- **Ошибки API:** Fallback на базовую обработку

## Troubleshooting

### Ошибки аутентификации
```
Error: 401 Unauthorized
```
**Решение:** Проверьте правильность всех 4 ключей

### Rate Limit ошибки
```
Error: 429 Too Many Requests
```
**Решение:** Подождите 15 минут или уменьшите частоту запросов

### Прокси ошибки
```
Error: Proxy authentication failed
```
**Решение:** Проверьте credentials прокси

### Недоступные твиты
```
Warning: Tweet 123456789 not found
```
**Возможные причины:**
- Твит удален
- Аккаунт заблокирован/приватный
- Неправильный ID твита

## Альтернативы

### Если Twitter API недоступен
1. **Отключите Twitter обработку** - бот будет работать с другими типами ссылок
2. **Используйте web scraping** - менее надежно, но не требует API
3. **Используйте сторонние API** - например, через RapidAPI

### Конфигурация без Twitter
```json
{
  "Twitter": {
    "ConsumerKey": "",
    "ConsumerSecret": "",
    "AccessToken": "",
    "AccessTokenSecret": ""
  }
}
```

При пустых ключах Twitter обработка будет пропускаться с логированием.

## Безопасность

### Хранение ключей
- ❌ Никогда не коммитьте ключи в git
- ✅ Используйте appsettings.Development.json (в .gitignore)
- ✅ Для продакшена используйте переменные окружения
- ✅ Регулярно ротируйте ключи

### Переменные окружения (продакшен)
```bash
export TWITTER__CONSUMERKEY="your-key"
export TWITTER__CONSUMERSECRET="your-secret"
export TWITTER__ACCESSTOKEN="your-token"
export TWITTER__ACCESSTOKENSECRET="your-token-secret"
```

---

**Статус:** Базовая структура готова, требует реализации реального API  
**Приоритет:** Средний (можно работать без Twitter)  
**Зависимости:** Twitter Developer Account, API ключи