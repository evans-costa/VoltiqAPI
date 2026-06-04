namespace Voltiq.Domain.Events;

public sealed record BudgetApprovedEvent(Guid BudgetId) : BaseDomainEvent;
