using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Common.Interfaces.Queue;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.GenerateBudgetPdf;

public record GenerateBudgetPdfCommand(Guid BudgetId) : IAuthenticatedRequest<ErrorOr<Success>>
{
    public Guid UserId { get; set; }
}

public class GenerateBudgetPdfCommandHandler(IQueueService queueService, IBudgetReadOnlyRepository budgetRepository)
    : IRequestHandler<GenerateBudgetPdfCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(GenerateBudgetPdfCommand request, CancellationToken cancellationToken)
    {
        var budget = await budgetRepository.GetByIdAndUserIdAsync(request.BudgetId, request.UserId, cancellationToken);
        if (budget == null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        if (budget.Status != BudgetStatus.Finalized)
            return Error.Validation(description: ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_GERAR_PDF);

        var message = new { request.BudgetId };

        await queueService.SendMessageAsync("budget-reports", message, cancellationToken);

        return Result.Success;
    }
}
