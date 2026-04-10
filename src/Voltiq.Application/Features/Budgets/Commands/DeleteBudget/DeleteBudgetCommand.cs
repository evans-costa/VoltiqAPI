using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Commands.DeleteBudget;

public sealed record DeleteBudgetCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Deleted>>
{
    public Guid UserId { get; set; }
}
