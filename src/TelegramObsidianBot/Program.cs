using Serilog;
using TelegramObsidianBot.Shared.Infrastructure;
using TelegramObsidianBot.Shared.Common;
using TelegramObsidianBot.Features.MessageProcessing.TelegramMessage;
using TelegramObsidianBot.Shared.Infrastructure.AI;
using TelegramObsidianBot.Features.ContentExtraction.TwitterContent;

// Настройка Serilog из конфигурации
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build())
  .CreateLogger();

try
{
  Log.Information("Starting TelegramObsidianBot application");

  var builder = WebApplication.CreateBuilder(args);

  // Добавляем Serilog
  builder.Host.UseSerilog();

  // Регистрируем конфигурации
  builder.Services.Configure<TelegramConfiguration>(
    builder.Configuration.GetSection("Telegram"));
  builder.Services.Configure<OpenRouterConfiguration>(
    builder.Configuration.GetSection("OpenRouter"));
  builder.Services.Configure<OllamaConfiguration>(
    builder.Configuration.GetSection("Ollama"));
  builder.Services.Configure<ObsidianConfiguration>(
    builder.Configuration.GetSection("Obsidian"));
  builder.Services.Configure<YouTubeConfiguration>(
    builder.Configuration.GetSection("YouTube"));
  builder.Services.Configure<TwitterCredentials>(
    builder.Configuration.GetSection("Twitter"));
  builder.Services.Configure<ProxyOptions>(
    builder.Configuration.GetSection("Proxy"));

  // Добавляем Rebus
  builder.Services.AddRebus();
  builder.Services.AddRebusHandlers();

  // Добавляем HTTP клиенты
  builder.Services.AddHttpClient();

  // Добавляем AI сервисы
  builder.Services.AddAIServices(builder.Configuration);

  // Добавляем фичи
  builder.Services.AddTelegramMessageFeature();
  builder.Services.AddTwitterContentFeature();

  var app = builder.Build();

  app.MapGet("/", () => "TelegramObsidianBot is running!");
  app.MapGet("/health", () => "Healthy");

  Log.Information("TelegramObsidianBot application started successfully");
  
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "TelegramObsidianBot application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
