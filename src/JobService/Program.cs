using System.Reflection;

using JobService.Background;
using JobService.Configurations;
using JobService.Database;

using MassTransit;

using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static bool _isDevelopment;

    private static void Main(string[] args)
    {
        Console.Title = Assembly.GetExecutingAssembly().GetName().Name!;

        IHostBuilder builder = CreateHostBuilder(args);

        IHost host = builder.Build();

        if(_isDevelopment)
        {
            host.InitializeDatabaseAsync().Wait();
        }

        host.Run();
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

            services.AddScoped<AppDbContextInitializer>();

            services.AddHostedService<PayrollBackgroundService>();
            services.AddHostedService<JobsBackgroundService>();

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

            _isDevelopment = hostContext.HostingEnvironment.IsDevelopment();
        });

        return builder;
    }
}