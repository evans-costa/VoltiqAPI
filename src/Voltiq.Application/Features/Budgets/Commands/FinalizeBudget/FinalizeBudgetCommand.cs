using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Budgets.Commands.FinalizeBudget;

public record FinalizeBudgetCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Success>>
{
    public Guid UserId { get; set; }
}
