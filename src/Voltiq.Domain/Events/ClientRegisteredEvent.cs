namespace Voltiq.Domain.Events;

public sealed record ClientRegisteredEvent(Guid ClientId) : BaseDomainEvent;
