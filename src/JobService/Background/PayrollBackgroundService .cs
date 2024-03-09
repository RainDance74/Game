using JobService.Database;
using JobService.Database.Models;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using UserService.Contracts;

namespace JobService.Background;

public class PayrollBackgroundService(IServiceProvider _serviceProvider,
    ILogger<PayrollBackgroundService> _logger)
    : BackgroundService
{
    private readonly Mutex _mutex = new(false, "Global\\PayrollBackgroundService_Mutex");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Payroll Background Service is launched.");

        _mutex.WaitOne();

        _logger.LogInformation(
            "Payroll Background Service is working.");

        while(!stoppingToken.IsCancellationRequested)
        {
            using(IServiceScope scope = _serviceProvider.CreateScope())
            {
                AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                IPublishEndpoint publishEndpointProvider = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                await Payroll(dbContext, publishEndpointProvider, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    public async Task Payroll(AppDbContext context, IPublishEndpoint publishEndpointProvider, CancellationToken stoppingToken)
    {
        Job[] jobs = await context.Jobs
            .AsNoTracking()
            .ToArrayAsync(stoppingToken);

        foreach(Job job in jobs)
        {
            await Task.WhenAll(job.Workers.Select(w => Payroll(w.Id, job.Salary)));
        }

        async Task Payroll(Guid worker, decimal salary) =>
            await publishEndpointProvider.Publish<IncrementUserBalance>(new(worker, salary), stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Payroll Background Service is stopping.");

        _mutex.ReleaseMutex();
        _mutex.Dispose();

        await base.StopAsync(stoppingToken);
    }
}
