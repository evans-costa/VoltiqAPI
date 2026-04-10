using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.DeleteBudget;

public sealed class DeleteBudgetCommandHandler(
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBudgetCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        budgetUpdateOnly.Remove(budget);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
