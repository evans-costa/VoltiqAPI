namespace Voltiq.Domain.Events;

public sealed record ClientCreatedEvent(Guid ClientId) : BaseDomainEvent;
