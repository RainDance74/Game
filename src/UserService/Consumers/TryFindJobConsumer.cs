using JobService.Contracts;

using MassTransit;

using UserService.Contracts;

namespace UserService.Consumers;
internal class TryFindJobConsumer(IRequestClient<GetPersonJob> _getPersonJobClient,
    IRequestClient<PickRandomJob> _pickRandomJobClient,
    IRequestClient<HirePerson> _hirePersonClient,
    ISendEndpoint _sendEndpoint)
    : IConsumer<TryFindJob>
{
    public async Task Consume(ConsumeContext<TryFindJob> context)
    {
        Response<JobReceived> userJobReceived= default!;

        Response<JobReceived> randomJobReceived = default!;

        Task[] findingJobsTasks = [
            Task.Factory.StartNew(async () => userJobReceived = await _getPersonJobClient.GetResponse<JobReceived>(new(context.Message.Id))),
            Task.Factory.StartNew(async () => randomJobReceived = await _pickRandomJobClient.GetResponse<JobReceived>(new()))
        ];

        await Task.WhenAll(findingJobsTasks);

        if(userJobReceived.Message.Salary > randomJobReceived.Message.Salary)
        {
            throw new Exception("Your salary is pretty good.. You don't need that.");
        }

        await _sendEndpoint.Send<FirePerson>(new(userJobReceived.Message.Id, context.Message.Id));

        Response<JobReceived> newJob = await _hirePersonClient.GetResponse<JobReceived>(new(randomJobReceived.Message.Id, context.Message.Id));

        await context.Send<JobFound>(new(newJob.Message.Id));
    }
}
