using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Voltiq.Application.Common.Interfaces.Reports;
using Voltiq.Application.Common.Interfaces.Storage;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Functions;

public class GenerateReportMessage
{
    public Guid BudgetId { get; set; }
}

public class GenerateReportFunction
{
    private readonly ILogger<GenerateReportFunction> _logger;
    private readonly IBudgetReadOnlyRepository _budgetRepository;
    private readonly IBudgetUpdateOnlyRepository _budgetUpdateRepository;
    private readonly IClientReadOnlyRepository _clientRepository;
    private readonly IReportGenerator _reportGenerator;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateReportFunction(
        ILogger<GenerateReportFunction> logger,
        IBudgetReadOnlyRepository budgetRepository,
        IBudgetUpdateOnlyRepository budgetUpdateRepository,
        IClientReadOnlyRepository clientRepository,
        IReportGenerator reportGenerator,
        IStorageService storageService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _budgetRepository = budgetRepository;
        _budgetUpdateRepository = budgetUpdateRepository;
        _clientRepository = clientRepository;
        _reportGenerator = reportGenerator;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    [Function(nameof(GenerateReportFunction))]
    public async Task Run([QueueTrigger("budget-reports")] string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing report generation message: {Message}", message);

        try
        {
            var msg = JsonSerializer.Deserialize<GenerateReportMessage>(message);
            if (msg == null || msg.BudgetId == Guid.Empty)
            {
                _logger.LogWarning("Invalid message format.");
                return;
            }

            var budget = await _budgetRepository.GetByIdAsync(msg.BudgetId, cancellationToken);
            if (budget == null)
            {
                _logger.LogWarning("Budget not found for ID: {BudgetId}", msg.BudgetId);
                return;
            }

            var client = await _clientRepository.GetByIdAndUserIdAsync(budget.ClientId, budget.UserId, cancellationToken);

            var reportData = new BudgetReportData
            {
                BudgetId = budget.Id,
                ProjectName = $"Orçamento #{budget.Id.ToString()[..8]}",
                ClientName = client?.Name ?? "Cliente não informado",
                TotalAmount = budget.TotalAmount,
                CreatedAt = budget.CreatedAt
            };

            _logger.LogInformation("Generating PDF for Budget ID: {BudgetId}", budget.Id);
            var pdfBytes = await _reportGenerator.GenerateAsync(reportData, cancellationToken);

            var fileName = $"budget-{budget.Id}.pdf";
            
            _logger.LogInformation("Uploading PDF for Budget ID: {BudgetId} to Blob Storage", budget.Id);
            var uri = await _storageService.UploadAsync(fileName, pdfBytes, "application/pdf", cancellationToken);

            budget.MarkAsPdfGenerated(uri);
            _budgetUpdateRepository.Update(budget);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Report generated and budget status updated successfully. URI: {Uri}", uri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing report generation for message: {Message}", message);
            throw;
        }
    }
}
