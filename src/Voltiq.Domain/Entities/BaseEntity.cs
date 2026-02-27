namespace Voltiq.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private readonly List<Events.IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<Events.IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(Events.IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
