namespace TelegramObsidianBot.Shared.Common;

public record TelegramConfiguration(
  string BotToken);

public record OpenRouterConfiguration(
  string ApiKey,
  string Model);

public record OllamaConfiguration(
  string Endpoint,
  string Model);

public record ObsidianConfiguration(
  string VaultPath,
  string DefaultFolder);

public record YouTubeConfiguration(
  string ApiKey);

public record TwitterCredentials(
  string ConsumerKey,
  string ConsumerSecret,
  string AccessToken,
  string AccessTokenSecret);

public record ProxyOptions(
  string Host,
  int Port,
  string Username,
  string Password);