using JobService.Contracts;

using MassTransit;

namespace JobService.Background;

public class JobsBackgroundService(IServiceProvider _serviceProvider,
    ILogger<JobsBackgroundService> _logger)
    : BackgroundService
{
    private readonly Mutex _mutex = new(false, "Global\\JobsBackgroundService_Mutex");
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Jobs Background Service is launched.");

        _mutex.WaitOne();

        _logger.LogInformation(
            "Jobs Background Service is working.");

        while(!stoppingToken.IsCancellationRequested)
        {
            using(IServiceScope scope = _serviceProvider.CreateScope())
            {
                IPublishEndpoint publishEndpointProvider = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                await CreateRandomJob(publishEndpointProvider, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(_random.Next(1, 10)), stoppingToken);
        }
    }

    public async Task CreateRandomJob(IPublishEndpoint publishEndpointProvider, CancellationToken stoppingToken)
    {
        var jobSalary = _random.Next(300, 3000);
        await publishEndpointProvider.Publish<CreateJob>(new(jobSalary), stoppingToken);
        _logger.LogInformation(
            "Jobs Background Service: new job with salary of {Salary} published to create.", jobSalary);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Jobs Background Service is stopping.");

        _mutex.ReleaseMutex();
        _mutex.Dispose();

        await base.StopAsync(stoppingToken);
    }
}
