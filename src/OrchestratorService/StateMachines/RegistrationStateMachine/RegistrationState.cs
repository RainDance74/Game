using MassTransit;

namespace OrchestratorService.StateMachines.RegistrationStateMachine;

public class RegistrationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public Guid UserId { get; set; }
    public Guid FutureUserJob { get; set; }
    public int CurrentState { get; set; }
}
