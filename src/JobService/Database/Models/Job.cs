namespace JobService.Database.Models;


public class Job
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public decimal Salary { get; set; }

    public List<Guid> Workers { get; } = [];
}