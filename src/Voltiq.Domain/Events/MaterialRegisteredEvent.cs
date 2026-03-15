namespace Voltiq.Domain.Events;

public sealed record MaterialRegisteredEvent(Guid MaterialId) : BaseDomainEvent;
