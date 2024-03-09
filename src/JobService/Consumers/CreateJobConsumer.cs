using JobService.Contracts;
using JobService.Database;
using JobService.Database.Models;

using MassTransit;

namespace JobService.Consumers;

public class CreateJobConsumer(AppDbContext _dbContext)
    : IConsumer<CreateJob>
{
    public async Task Consume(ConsumeContext<CreateJob> context)
    {
        var newJob = new Job
        {
            Salary = context.Message.Salary
        };

        await _dbContext.AddAsync(newJob);

        await _dbContext.SaveChangesAsync();

        await context.RespondAsync<JobCreated>(new(newJob.Id));
    }
}
