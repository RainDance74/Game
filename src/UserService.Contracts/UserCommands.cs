namespace UserService.Contracts
{
    public record CreateUser(string Username);
    public record GetUserBalance(Guid Id);
    public record IncrementUserBalance(Guid Id, decimal Amount);
    public record TryFindJob(Guid Id);
}
