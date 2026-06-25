using Voltiq.Application.Features.Budgets;
using Voltiq.Application.Features.Budgets.Commands.RegisterBudget;
using Voltiq.Application.Features.Budgets.Commands.UpdateBudget;
using Voltiq.Domain.Entities;

namespace Voltiq.Application.Mappings.Budgets;

public static class BudgetMappingExtensions
{
    extension(RegisterBudgetRequest request)
    {
        public RegisterBudgetCommand ToCommand() =>
            new(request.ClientId,
                request.Items
                    .Select(i => new RegisterBudgetItemCommand(
                        i.MaterialId, i.MaterialName, i.Type, i.Unit, i.Quantity, i.UnitPrice))
                    .ToList());
    }

    extension(UpdateBudgetRequest request)
    {
        public UpdateBudgetCommand ToCommand(Guid id) =>
            new(id,
                request.ClientId,
                request.Items
                    .Select(i => new UpdateBudgetItemCommand(
                        i.MaterialId, i.MaterialName, i.Type, i.Unit, i.Quantity, i.UnitPrice))
                    .ToList());
    }

    extension(Budget budget)
    {
        public BudgetSummaryResponse ToSummaryResponse() =>
            new(budget.Id, budget.Status, budget.PdfGenerationStatus, budget.TotalAmount, budget.CreatedAt,
                new BudgetClientSummaryResponse(budget.Client!.Id, budget.Client!.Name));

        public BudgetDetailResponse ToDetailResponse() =>
            budget.ToDetailResponse(budget.Client!);

        public BudgetDetailResponse ToDetailResponse(Client client) =>
            new(budget.Id, budget.Status, budget.PdfGenerationStatus, budget.TotalAmount, budget.CreatedAt,
                new BudgetClientDetailResponse(
                    client.Id, client.Name, client.Phone, client.Email.Value),
                budget.Items.Select(i => new BudgetItemResponse(
                    i.Id, i.MaterialId, i.MaterialName, i.Type, i.Unit,
                    i.Quantity, i.UnitPrice, i.TotalPrice)).ToList());
    }
}
