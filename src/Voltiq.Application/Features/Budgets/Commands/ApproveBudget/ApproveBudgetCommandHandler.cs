using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.ApproveBudget;

public sealed class ApproveBudgetCommandHandler(
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveBudgetCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        ApproveBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        budget.Approve();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
