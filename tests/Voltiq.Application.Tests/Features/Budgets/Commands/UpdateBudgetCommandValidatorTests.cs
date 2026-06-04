using FluentValidation.TestHelper;
using Voltiq.Application.Features.Budgets.Commands.UpdateBudget;
using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Budgets.Commands;

public class UpdateBudgetCommandValidatorTests
{
    private readonly UpdateBudgetCommandValidator _validator = new();

    private static UpdateBudgetCommand ValidCommand()
    {
        return new UpdateBudgetCommand(Guid.NewGuid(), Guid.NewGuid(), [
            new UpdateBudgetItemCommand(null, "Cabo 10mm", BudgetItemType.MaoDeObra,
                null, 2, 15.50m)
        ]);
    }

    [Fact]
    public void Validate_WithValidData_ShouldHaveNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyClientId_ShouldHaveError()
    {
        var command = ValidCommand() with { ClientId = Guid.Empty };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.ClientId)
            .WithErrorMessage(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);
    }

    [Fact]
    public void Validate_WithEmptyItems_ShouldHaveError()
    {
        var command = ValidCommand() with { Items = [] };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyItemMaterialName_ShouldHaveError(string? name)
    {
        var command = ValidCommand() with
        {
            Items =
            [
                new UpdateBudgetItemCommand(null, name!, BudgetItemType.MaoDeObra, null, 1,
                    10.00m)
            ]
        };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor("Items[0].MaterialName")
            .WithErrorMessage(ResourceErrorMessages.ORCAMENTO_ITEM_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithInvalidItemQuantity_ShouldHaveError(int quantity)
    {
        var command = ValidCommand() with
        {
            Items =
            [
                new UpdateBudgetItemCommand(null, "Cabo 10mm", BudgetItemType.MaoDeObra, null,
                    quantity, 15.50m)
            ]
        };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_WithInvalidItemUnitPrice_ShouldHaveError(double price)
    {
        var command = ValidCommand() with
        {
            Items =
            [
                new UpdateBudgetItemCommand(null, "Cabo 10mm", BudgetItemType.MaoDeObra, null, 1,
                    (decimal)price)
            ]
        };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor("Items[0].UnitPrice")
            .WithErrorMessage(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);
    }
}
