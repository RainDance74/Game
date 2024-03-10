using System.Reflection;

using MassTransit;

using Telegram.Bot;

using TgWorker.Background;
using TgWorker.Configurations;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Title = Assembly.GetExecutingAssembly().GetName().Name!;
        CreateHostBuilder(args)
            .Build()
            .Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((hostContext, services) =>
        {
            IConfigurationSection rabbitMqSection = hostContext.Configuration.GetSection("RabbitMqConfiguration");
            RabbitMqConfiguration? rabbitMqConfig = rabbitMqSection.Get<RabbitMqConfiguration>();

            var telegramToken = Environment.GetEnvironmentVariable("tg_token", EnvironmentVariableTarget.User)
                ?? throw new Exception("Token was not found in user environment variables");

            services.AddSingleton(new TelegramBotClient(telegramToken));

            services.AddSingleton<TgWorker.Services.JobService>();
            services.AddSingleton<TgWorker.Services.UserService>();

            services.AddHostedService<BotBackgroundService>();

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);

                    cfg.Host(rabbitMqConfig?.Hostname, rabbitMqConfig?.VirtualHost, h =>
                    {
                        h.Username(rabbitMqConfig?.Username);
                        h.Password(rabbitMqConfig?.Password);
                    });
                });
            });
        });

        return builder;
    }
}