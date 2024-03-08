using JobService.Contracts;

using MassTransit;

namespace JobService.Background;

public class JobsBackgroundService(ILogger<JobsBackgroundService> _logger,
    ISendEndpointProvider _sendEndpointProvider)
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
            await CreateRandomJob(stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(_random.Next(1, 10)), stoppingToken);
        }
    }

    public async Task CreateRandomJob(CancellationToken stoppingToken) =>
        await _sendEndpointProvider.Send<CreateJob>(new(_random.Next(300, 3000)), stoppingToken);

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Jobs Background Service is stopping.");

        _mutex.ReleaseMutex();
        _mutex.Dispose();

        await base.StopAsync(stoppingToken);
    }
}
