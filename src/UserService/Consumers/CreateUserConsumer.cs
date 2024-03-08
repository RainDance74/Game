using MassTransit;

using Microsoft.EntityFrameworkCore;

using UserService.Contracts;
using UserService.Database;
using UserService.Database.Models;

namespace UserService.Consumers;
public class CreateUserConsumer(AppDbContext _dbContext)
    : IConsumer<CreateUser>
{
    public async Task Consume(ConsumeContext<CreateUser> context)
    {
        var userExists = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.UserName == context.Message.Username) is not null;

        if(userExists)
        {
            throw new Exception("User with the same username already presented in the database.");
        }

        var newUser = new User { UserName = context.Message.Username };

        _dbContext.Users.Add(newUser);

        await _dbContext.SaveChangesAsync();

        await context.RespondAsync<UserCreated>(new(newUser.Id));
    }
}
