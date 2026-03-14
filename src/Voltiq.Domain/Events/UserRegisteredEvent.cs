namespace Voltiq.Domain.Events;

public sealed record UserRegisteredEvent(Guid UserId) : BaseDomainEvent;
