namespace Voltiq.Domain.Events;

public sealed record BudgetRejectedEvent(Guid BudgetId) : BaseDomainEvent;
