using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Budgets.Commands.UpdateBudget;

public sealed record UpdateBudgetRequest(
    Guid ClientId,
    IReadOnlyList<UpdateBudgetItemRequest> Items);

public sealed record UpdateBudgetItemRequest(
    Guid? MaterialId,
    string MaterialName,
    BudgetItemType Type,
    MaterialUnit? Unit,
    int Quantity,
    decimal UnitPrice);
