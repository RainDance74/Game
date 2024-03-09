using JobService.Contracts;

using MassTransit;

using UserService.Contracts;

namespace OrchestratorService.StateMachines.RegistrationStateMachine;

public class RegistrationStateMachine : MassTransitStateMachine<RegistrationState>
{
    private readonly ILogger<RegistrationStateMachine> _logger;

    public Event<CreateUser> RegistrationRequested { get; private set; } = default!;
    public State FindingJob { get; private set; } = default!;
    public State AwaitingJob { get; private set; } = default!;
    public Request<RegistrationState, CreateUser, UserCreated> CreateUserRequest { get; private set; } = default!;
    public Request<RegistrationState, PickRandomJob, JobReceived> PickRandomJobRequest { get; private set; } = default!;

    public RegistrationStateMachine(ILogger<RegistrationStateMachine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InstanceState(x => x.CurrentState);

        Event(() => RegistrationRequested, x => x.CorrelateById(context => NewId.NextGuid()));
        Request(() => CreateUserRequest, x => x.Timeout = TimeSpan.FromMinutes(1));
        Request(() => PickRandomJobRequest, x => x.Timeout = TimeSpan.FromMinutes(1));

        Initially(
            When(RegistrationRequested)
                .Then(context =>
                {
                    _logger.LogInformation("Registration requested for username: {Username}", context.Message.Username);
                })
                .Request(CreateUserRequest, context => context.Init<CreateUser>(new
                {
                    Username = context.Message.Username
                }))
                .TransitionTo(FindingJob)
        );

        During(FindingJob,
            When(CreateUserRequest.Completed)
                .Then(context =>
                {
                    context.Saga.UserId = context.Message.Id;
                    _logger.LogInformation("[{Date}][SAGA] Got user. CorrelationId: {Id}", DateTime.Now, context.Saga.CorrelationId);
                })
                .Request(PickRandomJobRequest, context => context.Init<PickRandomJob>(new { }))
                .TransitionTo(AwaitingJob));

        During(AwaitingJob,
            When(PickRandomJobRequest.Completed)
                .Then(context =>
                {
                    context.Saga.FutureUserJob = context.Message.Id;
                    _logger.LogDebug("[{Date}][SAGA] Found future job - {Job}, for user: {User}", DateTime.Now, context.Saga.FutureUserJob, context.Saga.UserId);
                })
                .PublishAsync(context => context.Init<HirePerson>(new
                {
                    JobId = context.Saga.FutureUserJob,
                    PersonId = context.Saga.UserId
                }))
                .Finalize());

        SetCompletedWhenFinalized();
    }
}

