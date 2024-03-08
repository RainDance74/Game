using JobService.Contracts;
using JobService.Database;
using JobService.Database.Models;

using MassTransit;

namespace JobService.Consumers;

public class HirePersonConsumer(AppDbContext _dbContext)
    : IConsumer<HirePerson>
{
    public async Task Consume(ConsumeContext<HirePerson> context)
    {
        Job job = await _dbContext.FindAsync<Job>([context.Message.JobId])
            ?? throw new Exception("There is no job with this id!");

        job.Workers.Add(context.Message.PersonId);

        await _dbContext.SaveChangesAsync();
    }
}
