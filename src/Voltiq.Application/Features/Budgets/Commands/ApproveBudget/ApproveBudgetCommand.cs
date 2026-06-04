using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Commands.ApproveBudget;

public record ApproveBudgetCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Updated>>
{
    public Guid UserId { get; set; }
}
