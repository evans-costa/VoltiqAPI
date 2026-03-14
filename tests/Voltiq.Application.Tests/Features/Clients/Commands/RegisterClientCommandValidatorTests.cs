using FluentValidation.TestHelper;
using Voltiq.Application.Features.Clients.Commands.RegisterClient;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Clients.Commands;

public class RegisterClientCommandValidatorTests
{
    private readonly RegisterClientCommandValidator _validator = new();

    private static RegisterClientCommand ValidCommand() =>
        new("João Silva", "(11) 99999-9999", "Rua das Flores", "123", "São Paulo", "SP", "01310-100");

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
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyPhone_ShouldHaveError(string? phone)
    {
        var command = ValidCommand() with { Phone = phone! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyStreet_ShouldHaveError(string? street)
    {
        var command = ValidCommand() with { Street = street! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Street)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_ENDERECO_LOGRADOURO_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyNumber_ShouldHaveError(string? number)
    {
        var command = ValidCommand() with { Number = number! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Number)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_ENDERECO_NUMERO_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyCity_ShouldHaveError(string? city)
    {
        var command = ValidCommand() with { City = city! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_ENDERECO_CIDADE_OBRIGATORIA);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyState_ShouldHaveError(string? state)
    {
        var command = ValidCommand() with { State = state! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.State)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_ENDERECO_ESTADO_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyZipCode_ShouldHaveError(string? zipCode)
    {
        var command = ValidCommand() with { ZipCode = zipCode! };
        _validator.TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.ZipCode)
            .WithErrorMessage(ResourceErrorMessages.CLIENTE_ENDERECO_CEP_OBRIGATORIO);
    }
}
