using MassTransit;

using UserService.Contracts;
using UserService.Database;
using UserService.Database.Models;

namespace UserService.Consumers;

public class IncrementUserBalanceConsumer(AppDbContext _dbContext)
    : IConsumer<IncrementUserBalance>
{
    public async Task Consume(ConsumeContext<IncrementUserBalance> context)
    {
        User user = await _dbContext.FindAsync<User>([context.Message.Id])
            ?? throw new Exception("Couldn't find target user.");

        user.Balance += context.Message.Amount;

        await _dbContext.SaveChangesAsync();
    }
}
