using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Budgets;
using Voltiq.Domain.Interfaces.Repositories.Budget;

namespace Voltiq.Application.Features.Budgets.Queries.GetBudgets;

public sealed class GetBudgetsQueryHandler(IBudgetReadOnlyRepository budgetReadOnly)
    : IRequestHandler<GetBudgetsQuery, ErrorOr<IReadOnlyList<BudgetSummaryResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<BudgetSummaryResponse>>> Handle(
        GetBudgetsQuery query, CancellationToken cancellationToken)
    {
        var budgets = await budgetReadOnly.GetByUserIdWithClientAsync(
            query.UserId, cancellationToken);

        return budgets.Select(b => b.ToSummaryResponse()).ToList();
    }
}
