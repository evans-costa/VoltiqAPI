using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Tests.Entities;

public class BudgetItemTests
{
    private static readonly Guid ValidBudgetId = Guid.NewGuid();
    private static readonly Guid ValidMaterialId = Guid.NewGuid();
    private const string VALID_NAME = "Cabo 10mm";

    private static BudgetItem ValidMaterialItemCreate() =>
        BudgetItem.Create(ValidBudgetId, ValidMaterialId, BudgetItemType.Material, MaterialUnit.Metro, 2, 15.50m, VALID_NAME);

    [Fact]
    public void Create_MaterialTypeWithValidData_ShouldSucceed()
    {
        var item = ValidMaterialItemCreate();

        item.Type.ShouldBe(BudgetItemType.Material);
        item.MaterialId.ShouldNotBe(null);
        item.MaterialId.ShouldBe(ValidMaterialId);
        item.Unit.ShouldBe(MaterialUnit.Metro);
    }

    [Fact]
    public void Create_MaoDeObraTypeWithValidData_ShouldSucceed()
    {
        var item = BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, null, 8, 120m, "Mão de obra elétrica");

        item.Type.ShouldBe(BudgetItemType.MaoDeObra);
        item.MaterialId.ShouldBeNull();
        item.Unit.ShouldBeNull();
    }

    [Fact]
    public void Create_OutrosTypeWithValidData_ShouldSucceed()
    {
        var item = BudgetItem.Create(ValidBudgetId, null, BudgetItemType.Outros, null, 1, 50m, "Deslocamento");

        item.Type.ShouldBe(BudgetItemType.Outros);
        item.MaterialId.ShouldBeNull();
        item.Unit.ShouldBeNull();
    }

    [Fact]
    public void Create_MaterialTypeWithoutMaterialId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, null, BudgetItemType.Material, MaterialUnit.Metro, 2, 15.50m, VALID_NAME))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEM_MATERIAL_ID_OBRIGATORIO_PARA_MATERIAL);
    }

    [Fact]
    public void Create_MaterialTypeWithoutUnit_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, ValidMaterialId, BudgetItemType.Material, null, 2, 15.50m, VALID_NAME))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEM_UNIDADE_OBRIGATORIA_PARA_MATERIAL);
    }

    [Fact]
    public void Create_MaoDeObraTypeWithMaterialId_ShouldThrowDomainException()
    {
        var ex = Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, ValidMaterialId, BudgetItemType.MaoDeObra, null, 8, 120m, "Mão de obra"));

        ex.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_MaoDeObraTypeWithUnit_ShouldThrowDomainException()
    {
        var ex = Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, MaterialUnit.Metro, 8, 120m, "Mão de obra"));

        ex.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_OutrosTypeWithMaterialId_ShouldThrowDomainException()
    {
        var ex = Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, ValidMaterialId, BudgetItemType.Outros, null, 1, 50m, "Deslocamento"));

        ex.Message.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, null, 2, 10m, ""))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEM_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidQuantity_ShouldThrowDomainException(int quantity)
    {
        Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, null, quantity, 10m, VALID_NAME))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidUnitPrice_ShouldThrowDomainException(decimal unitPrice)
    {
        Should.Throw<DomainException>(() =>
            BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, null, 3, unitPrice, VALID_NAME))
            .Message.ShouldBe(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);
    }

    [Fact]
    public void Create_MaoDeObraType_ShouldCalculateTotalPriceCorrectly()
    {
        var item = BudgetItem.Create(ValidBudgetId, null, BudgetItemType.MaoDeObra, null, 3, 10m, "Instalação");

        item.TotalPrice.ShouldBe(30m);
    }
}
