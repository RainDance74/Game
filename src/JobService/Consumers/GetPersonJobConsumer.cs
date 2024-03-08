using JobService.Contracts;
using JobService.Database;
using JobService.Database.Models;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace JobService.Consumers;

public class GetPersonJobConsumer(AppDbContext _dbContext)
    : IConsumer<GetPersonJob>
{
    public async Task Consume(ConsumeContext<GetPersonJob> context)
    {
        Job userJob = await _dbContext.Jobs
            .FirstOrDefaultAsync(j => j.Workers.Contains(context.Message.PersonId))
            ?? throw new Exception("Job wasn't found.");

        await context.RespondAsync<JobReceived>(new(userJob.Id, userJob.Salary));
    }
}
