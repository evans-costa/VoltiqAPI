namespace Voltiq.Domain.Events;

public sealed record BudgetRegisteredEvent(Guid BudgetId) : BaseDomainEvent;
