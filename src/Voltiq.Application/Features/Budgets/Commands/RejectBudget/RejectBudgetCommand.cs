using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Commands.RejectBudget;

public record RejectBudgetCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Updated>>
{
    public Guid UserId { get; set; }
}
