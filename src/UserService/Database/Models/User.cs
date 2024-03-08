namespace UserService.Database.Models;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string UserName { get; set; } = default!;

    public decimal Balance { get; set; }
}
