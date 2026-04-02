using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Queries.GetBudgetById;

public sealed record GetBudgetByIdQuery(Guid Id) : IAuthenticatedRequest<ErrorOr<BudgetDetailResponse>>
{
    public Guid UserId { get; set; }
}
