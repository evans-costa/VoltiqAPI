using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces.Queue;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.FinalizeBudget;

public sealed class FinalizeBudgetCommandHandler(
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork,
    IQueueService queueService)
    : IRequestHandler<FinalizeBudgetCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(
        FinalizeBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdWithItemsAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        budget.FinalizeBudget();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var message = new { BudgetId = budget.Id };
        await queueService.SendMessageAsync("budget-reports", message, cancellationToken);

        return Result.Success;
    }
}
