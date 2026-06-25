using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Budgets;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.RegisterBudget;

public sealed class RegisterBudgetCommandHandler(
    IClientReadOnlyRepository clientReadOnly,
    IMaterialReadOnlyRepository materialReadOnly,
    IBudgetWriteOnlyRepository budgetWriteOnly,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterBudgetCommand, ErrorOr<BudgetDetailResponse>>
{
    public async Task<ErrorOr<BudgetDetailResponse>> Handle(
        RegisterBudgetCommand command, CancellationToken cancellationToken)
    {
        var client = await clientReadOnly.GetByIdAndUserIdAsync(
            command.ClientId, command.UserId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        var materialLookup = new Dictionary<Guid, Material>();
        foreach (var materialId in command.Items.Where(i => i.MaterialId.HasValue).Select(i => i.MaterialId!.Value).Distinct())
        {
            var material = await materialReadOnly.GetByIdAndUserIdAsync(
                materialId, command.UserId, cancellationToken);

            if (material is null)
                return Error.NotFound(description: ResourceErrorMessages.MATERIAL_NAO_ENCONTRADO);

            materialLookup[materialId] = material;
        }

        var budget = Budget.Register(command.UserId, command.ClientId);

        foreach (var item in command.Items)
        {
            var budgetItem = BudgetItem.Create(
                budget.Id, item.MaterialId, item.Type, item.Unit, item.Quantity, item.UnitPrice, item.MaterialName);

            budget.AddItem(budgetItem);
        }

        await budgetWriteOnly.AddAsync(budget, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return budget.ToDetailResponse(client);
    }
}
