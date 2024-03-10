using System.Reflection;

using MassTransit;

using OrchestratorService.Configurations;
using OrchestratorService.StateMachines.RegistrationStateMachine;

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

            services.AddMassTransit(x =>
            {
                x.AddSagaRepository<RegistrationState>()
                    .InMemoryRepository();

                x.AddSagaStateMachine<RegistrationStateMachine, RegistrationState>()
                    .InMemoryRepository();

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