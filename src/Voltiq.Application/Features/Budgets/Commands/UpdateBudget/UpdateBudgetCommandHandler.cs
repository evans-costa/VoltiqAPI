using ErrorOr;
using MediatR;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.UpdateBudget;

public sealed class UpdateBudgetCommandHandler(
    IClientReadOnlyRepository clientReadOnly,
    IMaterialReadOnlyRepository materialReadOnly,
    IBudgetUpdateOnlyRepository budgetUpdateOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBudgetCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateBudgetCommand command, CancellationToken cancellationToken)
    {
        var budget = await budgetUpdateOnly.GetTrackedByIdWithItemsAndUserIdAsync(
            command.Id, command.UserId, cancellationToken);

        if (budget is null)
            return Error.NotFound(description: ResourceErrorMessages.ORCAMENTO_NAO_ENCONTRADO);

        var client = await clientReadOnly.GetByIdAndUserIdAsync(
            command.ClientId, command.UserId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        var budgetItems = new List<BudgetItem>();
        foreach (var item in command.Items)
        {
            if (item.MaterialId.HasValue)
            {
                var material = await materialReadOnly.GetByIdAndUserIdAsync(
                    item.MaterialId.Value, command.UserId, cancellationToken);

                if (material is null)
                    return Error.NotFound(description: ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);
            }

            var budgetItem = BudgetItem.Create(
                budget.Id, item.MaterialId, item.Type, item.Unit, item.Quantity, item.UnitPrice, item.MaterialName);

            budgetItems.Add(budgetItem);
        }

        budget.Edit(command.ClientId, budgetItems);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
