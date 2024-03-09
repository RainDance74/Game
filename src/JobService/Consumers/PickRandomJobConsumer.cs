using JobService.Contracts;
using JobService.Database;
using JobService.Database.Models;

using MassTransit;

using Microsoft.EntityFrameworkCore;

namespace JobService.Consumers;

public class PickRandomJobConsumer(AppDbContext _dbContext)
    : IConsumer<PickRandomJob>
{
    public async Task Consume(ConsumeContext<PickRandomJob> context)
    {
        Job randomJob = await _dbContext.Jobs
            .OrderBy(j => Guid.NewGuid())
            .FirstOrDefaultAsync()
            ?? throw new Exception("There is no any jobs!");

        await context.RespondAsync<JobReceived>(new(randomJob.Id, randomJob.Salary));
    }
}
