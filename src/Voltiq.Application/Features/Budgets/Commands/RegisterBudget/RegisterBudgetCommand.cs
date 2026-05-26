using ErrorOr;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Budgets.Commands.RegisterBudget;

public sealed record RegisterBudgetCommand(
    Guid ClientId,
    IReadOnlyList<RegisterBudgetItemCommand> Items) : IAuthenticatedRequest<ErrorOr<BudgetDetailResponse>>
{
    public Guid UserId { get; set; }
}

public sealed record RegisterBudgetItemCommand(
    Guid? MaterialId,
    string MaterialName,
    BudgetItemType Type,
    MaterialUnit? Unit,
    int Quantity,
    decimal UnitPrice);
