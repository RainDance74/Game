﻿namespace UserService.Contracts
{
    public record UserCreated(Guid Id);
    public record UserBalanceReceived(Guid Id, decimal Amount);
    public record JobFound(Guid? JobId);
}
