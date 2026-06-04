namespace Voltiq.Domain.Events;

public sealed record BudgetFinalizedEvent(Guid BudgetId) : BaseDomainEvent;
