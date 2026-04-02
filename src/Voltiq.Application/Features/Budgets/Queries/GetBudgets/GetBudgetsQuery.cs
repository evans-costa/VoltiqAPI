using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Queries.GetBudgets;

public sealed record GetBudgetsQuery : IAuthenticatedRequest<ErrorOr<IReadOnlyList<BudgetSummaryResponse>>>
{
    public Guid UserId { get; set; }
}
