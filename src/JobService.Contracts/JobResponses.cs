namespace JobService.Contracts
{
    public record JobCreated(Guid Id);
    public record JobReceived(Guid Id, decimal Salary);
}
