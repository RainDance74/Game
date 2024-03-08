namespace JobService.Contracts
{
    public record CreateJob(decimal Salary);
    public record PickRandomJob();
    public record HirePerson(Guid JobId, Guid PersonId);
    public record FirePerson(Guid JobId, Guid PersonId);
    public record GetPersonJob(Guid PersonId);
}
