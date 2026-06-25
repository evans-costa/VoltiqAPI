using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces.Storage;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Queries.GetBudgetPdfUrl;

public record GetBudgetPdfUrlQuery(Guid BudgetId) : IRequest<ErrorOr<string>>;

public class GetBudgetPdfUrlQueryHandler : IRequestHandler<GetBudgetPdfUrlQuery, ErrorOr<string>>
{
    private readonly IStorageService _storageService;
    private readonly IBudgetReadOnlyRepository _budgetRepository;

    public GetBudgetPdfUrlQueryHandler(IStorageService storageService, IBudgetReadOnlyRepository budgetRepository)
    {
        _storageService = storageService;
        _budgetRepository = budgetRepository;
    }

    public async Task<ErrorOr<string>> Handle(GetBudgetPdfUrlQuery request, CancellationToken cancellationToken)
    {
        var budget = await _budgetRepository.GetByIdAsync(request.BudgetId, cancellationToken);
        if (budget == null)
            return Error.NotFound(description: ResourceErrorMessages.TITULO_NAO_ENCONTRADO);

        var fileName = $"budget-{request.BudgetId}.pdf";
        var url = await _storageService.GetSasUrlAsync(fileName, 1, cancellationToken);

        if (string.IsNullOrEmpty(url))
            return Error.NotFound(code: "Budget.PdfNotGenerated", description: "O PDF ainda não foi gerado ou não está disponível.");

        return url;
    }
}
