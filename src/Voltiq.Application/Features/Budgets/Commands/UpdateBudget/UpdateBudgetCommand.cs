using ErrorOr;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Budgets.Commands.UpdateBudget;

public sealed record UpdateBudgetCommand(
    Guid Id,
    Guid ClientId,
    IReadOnlyList<UpdateBudgetItemCommand> Items) : IAuthenticatedRequest<ErrorOr<Updated>>
{
    public Guid UserId { get; set; }
}

public sealed record UpdateBudgetItemCommand(
    Guid? MaterialId,
    string MaterialName,
    BudgetItemType Type,
    MaterialUnit? Unit,
    int Quantity,
    decimal UnitPrice);
