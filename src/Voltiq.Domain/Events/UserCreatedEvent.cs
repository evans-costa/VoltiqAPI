namespace Voltiq.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId) : BaseDomainEvent;
