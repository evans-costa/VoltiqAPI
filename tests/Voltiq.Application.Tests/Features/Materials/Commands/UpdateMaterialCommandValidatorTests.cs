using FluentValidation.TestHelper;
using Voltiq.Application.Features.Materials.Commands.UpdateMaterial;
using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Materials.Commands;

public class UpdateMaterialCommandValidatorTests
{
    private readonly UpdateMaterialCommandValidator _validator = new();

    private static UpdateMaterialCommand ValidCommand() =>
        new(Guid.NewGuid(), "Cabo 10mm", 15.50m, MaterialUnit.Metro);

    [Fact]
    public void Validate_WithValidData_ShouldHaveNoErrors()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = ValidCommand() with { Name = name! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        var command = ValidCommand() with { DefaultPrice = price };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.DefaultPrice)
            .WithErrorMessage(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);
    }

    [Fact]
    public void Validate_WithInvalidUnit_ShouldHaveError()
    {
        var command = ValidCommand() with { Unit = (MaterialUnit)999 };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Unit)
            .WithErrorMessage(ResourceErrorMessages.MATERIAL_UNIDADE_INVALIDA);
    }
}
