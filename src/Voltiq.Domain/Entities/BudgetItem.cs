using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class BudgetItem : BaseEntity
{
    public Guid BudgetId { get; private set; }
    public Guid? MaterialId { get; private set; }
    public string MaterialName { get; private set; } = null!;
    public BudgetItemType Type { get; private set; }
    public MaterialUnit? Unit { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    private BudgetItem() { }

    private BudgetItem(
        Guid budgetId,
        Guid? materialId,
        BudgetItemType type,
        MaterialUnit? unit,
        int quantity,
        decimal unitPrice,
        string materialName)
    {
        BudgetId = budgetId;
        MaterialId = materialId;
        Type = type;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
        MaterialName = materialName;
    }

    public static BudgetItem Create(
        Guid budgetId,
        Guid? materialId,
        BudgetItemType type,
        MaterialUnit? unit,
        int quantity,
        decimal unitPrice,
        string materialName)
    {
        if (string.IsNullOrWhiteSpace(materialName))
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_NOME_OBRIGATORIO);

        if (quantity <= 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);

        if (unitPrice <= 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);

        if (type == BudgetItemType.Material)
        {
            if (materialId is null)
                throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_MATERIAL_ID_OBRIGATORIO_PARA_MATERIAL);

            if (unit is null)
                throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_UNIDADE_OBRIGATORIA_PARA_MATERIAL);
        }
        else
        {
            if (materialId is not null)
                throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_MATERIAL_ID_DEVE_SER_NULO);

            if (unit is not null)
                throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEM_UNIDADE_DEVE_SER_NULA);
        }

        return new BudgetItem(budgetId, materialId, type, unit, quantity, unitPrice, materialName.Trim());
    }
}
