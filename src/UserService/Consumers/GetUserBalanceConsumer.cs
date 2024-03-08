using MassTransit;

using UserService.Contracts;
using UserService.Database;
using UserService.Database.Models;

namespace UserService.Consumers;

public class GetUserBalanceConsumer(AppDbContext _dbContext)
    : IConsumer<GetUserBalance>
{
    public async Task Consume(ConsumeContext<GetUserBalance> context)
    {
        User user = await _dbContext.FindAsync<User>([context.Message.Id])
            ?? throw new Exception("There is no user with this Id.");

        await context.RespondAsync<UserBalanceReceived>(new(user.Id, user.Balance));
    }
}
