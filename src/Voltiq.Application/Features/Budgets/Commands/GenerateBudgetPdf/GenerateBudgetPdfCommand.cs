using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces.Queue;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.GenerateBudgetPdf;

public record GenerateBudgetPdfCommand(Guid BudgetId) : IRequest<ErrorOr<Success>>;

public class GenerateBudgetPdfCommandHandler : IRequestHandler<GenerateBudgetPdfCommand, ErrorOr<Success>>
{
    private readonly IQueueService _queueService;
    private readonly IBudgetReadOnlyRepository _budgetRepository;

    public GenerateBudgetPdfCommandHandler(IQueueService queueService, IBudgetReadOnlyRepository budgetRepository)
    {
        _queueService = queueService;
        _budgetRepository = budgetRepository;
    }

    public async Task<ErrorOr<Success>> Handle(GenerateBudgetPdfCommand request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, cancellationToken);
        if (budget == null)
            return Error.NotFound(description: ResourceErrorMessages.TITULO_NAO_ENCONTRADO);

        var message = new { request.BudgetId };

        await _queueService.SendMessageAsync("budget-reports", message, cancellationToken);

        return Result.Success;
    }
}
