using MassTransit;

using UserService.Contracts;

namespace TgWorker.Services;

public class UserService(IServiceProvider serviceProvider)
{
    public async Task RegisterUserAsync(string username)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        await publishEndpoint.Publish<CreateUser>(new(username));
    }
}
