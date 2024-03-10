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

        Event(() => RegistrationRequested);
        Request(() => CreateUserRequest, x => x.Timeout = TimeSpan.FromMinutes(1));
        Request(() => PickRandomJobRequest, x => x.Timeout = TimeSpan.FromMinutes(1));

        Initially(
            When(RegistrationRequested)
                .Then(context =>
                {
                    _logger.LogInformation("[{Date}][SAGA] Registration requested for username: {Username}. CorrelationId: {CorrelationId}",
                        DateTime.Now, context.Message.Username, context.CorrelationId);
                })
                .Request(CreateUserRequest, context => context.Init<CreateUser>(new
                {
                    context.Message.Username,
                    __CorelationId = context.Saga.CorrelationId
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
                .Request(PickRandomJobRequest, context => context.Init<PickRandomJob>(new
                {
                    __CorelationId = context.Saga.CorrelationId
                }))
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
                    PersonId = context.Saga.UserId,
                    __CorelationId = context.Saga.CorrelationId
                }))
                .Finalize());

        SetCompletedWhenFinalized();
    }
}

