using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.RejectBudget;

public sealed class RejectBudgetCommandHandler(
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectBudgetCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        RejectBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        budget.Reject();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
