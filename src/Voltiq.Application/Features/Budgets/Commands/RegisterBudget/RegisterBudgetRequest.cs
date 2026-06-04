using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Budgets.Commands.RegisterBudget;

public sealed record RegisterBudgetRequest(
    Guid ClientId,
    IReadOnlyList<RegisterBudgetItemRequest> Items);

public sealed record RegisterBudgetItemRequest(
    Guid? MaterialId,
    string MaterialName,
    BudgetItemType Type,
    MaterialUnit? Unit,
    int Quantity,
    decimal UnitPrice);
