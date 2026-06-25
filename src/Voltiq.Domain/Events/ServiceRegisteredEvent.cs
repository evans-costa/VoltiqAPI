namespace Voltiq.Domain.Events;

public sealed record ServiceRegisteredEvent(Guid ServiceId) : BaseDomainEvent;
