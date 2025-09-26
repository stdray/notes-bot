using FluentValidation;
using TelegramObsidianBot.Shared.Contracts;

namespace TelegramObsidianBot.Features.ContentExtraction.TwitterContent;

/// <summary>
/// Валидатор для Twitter URL
/// </summary>
public class TwitterUrlValidator : AbstractValidator<TwitterLinkDetected>
{
  public TwitterUrlValidator()
  {
    RuleFor(x => x.Url)
      .NotEmpty()
      .WithMessage("Twitter URL cannot be empty")
      .Must(BeValidTwitterUrl)
      .WithMessage("URL must be a valid Twitter/X.com link");

    RuleFor(x => x.ChatId)
      .GreaterThan(0)
      .WithMessage("ChatId must be positive");

    RuleFor(x => x.MessageId)
      .NotEmpty()
      .WithMessage("MessageId cannot be empty");

    RuleFor(x => x.CorrelationId)
      .NotEmpty()
      .WithMessage("CorrelationId cannot be empty");
  }

  private static bool BeValidTwitterUrl(string url)
  {
    if (string.IsNullOrEmpty(url))
      return false;

    return url.Contains("twitter.com/") || url.Contains("x.com/");
  }
}