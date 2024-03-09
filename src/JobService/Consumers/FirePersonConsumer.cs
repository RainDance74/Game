using JobService.Contracts;
using JobService.Database;
using JobService.Database.Models;

using MassTransit;

namespace JobService.Consumers;

public class FirePersonConsumer(AppDbContext _dbContext)
    : IConsumer<FirePerson>
{
    public async Task Consume(ConsumeContext<FirePerson> context)
    {
        Job job = await _dbContext.FindAsync<Job>([context.Message.JobId])
            ?? throw new Exception("There is no job with this id!");

        job.Workers.Remove(new(context.Message.PersonId));

        await _dbContext.SaveChangesAsync();
    }
}
