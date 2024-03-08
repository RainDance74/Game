using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using UserService.Configurations;
using UserService.Database;

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
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                options.UseNpgsql(connectionString);
            });

            IConfigurationSection rabbitMqSection = hostContext.Configuration.GetSection("RabbitMqConfiguration");
            RabbitMqConfiguration? rabbitMqConfig = rabbitMqSection.Get<RabbitMqConfiguration>();

            services.AddMassTransit(x =>
            {
                x.AddConsumers(Assembly.GetExecutingAssembly());

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