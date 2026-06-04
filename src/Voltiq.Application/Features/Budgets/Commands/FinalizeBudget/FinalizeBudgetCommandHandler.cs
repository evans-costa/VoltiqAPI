using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.FinalizeBudget;

public sealed class FinalizeBudgetCommandHandler(
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<FinalizeBudgetCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        FinalizeBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdWithItemsAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        budget.FinalizeBudget();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
