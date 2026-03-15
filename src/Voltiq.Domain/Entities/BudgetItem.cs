using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class BudgetItem : BaseEntity
{
    public Guid BudgetId { get; private set; }
    public Guid? MaterialId { get; private set; }
    public string MaterialName { get; private set; } = null!;
    public MaterialUnit? Unit { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    private BudgetItem() { }

    private BudgetItem(
        Guid budgetId,
        Guid? materialId,
        string materialName,
        MaterialUnit? unit,
        int quantity,
        decimal unitPrice)
    {
        BudgetId = budgetId;
        MaterialId = materialId;
        MaterialName = materialName;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
    }

    public static BudgetItem Create(
        Guid budgetId,
        Guid? materialId,
        string materialName,
        MaterialUnit? unit,
        int quantity,
        decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(materialName))
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_NOME_OBRIGATORIO);

        if (quantity <= 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);

        if (unitPrice <= 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);

        return new BudgetItem(budgetId, materialId, materialName.Trim(), unit, quantity, unitPrice);
    }
}
