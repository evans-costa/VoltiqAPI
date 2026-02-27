namespace Voltiq.Domain.Events;

public abstract record BaseDomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
