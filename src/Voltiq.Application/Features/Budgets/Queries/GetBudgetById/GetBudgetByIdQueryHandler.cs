using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Budgets;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Queries.GetBudgetById;

public sealed class GetBudgetByIdQueryHandler(IBudgetReadOnlyRepository budgetReadOnly)
    : IRequestHandler<GetBudgetByIdQuery, ErrorOr<BudgetDetailResponse>>
{
    public async Task<ErrorOr<BudgetDetailResponse>> Handle(
        GetBudgetByIdQuery query, CancellationToken cancellationToken)
    {
        var budget = await budgetReadOnly.GetByIdWithItemsAndClientAsync(
            query.Id, query.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        return budget.ToDetailResponse();
    }
}
