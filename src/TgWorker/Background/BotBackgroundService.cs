using Telegram.Bot;
using Telegram.Bot.Types;

namespace TgWorker.Background;

public class BotBackgroundService(TelegramBotClient _botClient,
    Services.UserService userService)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(HandleUpdate, ErrorHandler, cancellationToken: stoppingToken);
        return Task.CompletedTask;
    }

    private async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken token)
    {
        if(update.Message?.Text is null)
        {
            await Task.FromException(new Exception("Message should have text."));
        }

        if(update.Message?.Text == "/start")
        {
            await userService.RegisterUserAsync(update.Message.From?.Username
                ?? throw new Exception("There was no user in the message."));
            return;
        }
    }
    private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        Console.WriteLine($"Error occurred: {exception.Message}");
        return Task.CompletedTask;
    }
}
