using FluentValidation.TestHelper;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldHaveNoErrors()
    {
        var command = new CreateUserCommand(
            Name: "João Silva",
            Email: "joao@example.com",
            Document: "529.982.247-25",
            Password: "S3cur3P@ssw0rd!");

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveError(string? name)
    {
        var command = new CreateUserCommand(name!, "joao@example.com", "52998224725", "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullEmail_ShouldHaveRequiredEmailError(string? email)
    {
        var command = new CreateUserCommand("João", email!, "52998224725", "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveInvalidEmailError()
    {
        var command = new CreateUserCommand("João", "notanemail", "52998224725", "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullDocument_ShouldHaveRequiredDocumentError(string? document)
    {
        var command = new CreateUserCommand("João", "joao@example.com", document!, "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Document)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_OBRIGATORIO);
    }

    [Fact]
    public void Validate_WithInvalidDocumentFormat_ShouldHaveInvalidDocumentError()
    {
        var command = new CreateUserCommand("João", "joao@example.com", "12345", "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Document)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_INVALIDO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullPassword_ShouldHaveRequiredPasswordError(string? password)
    {
        var command = new CreateUserCommand("João", "joao@example.com", "52998224725", password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_SENHA_OBRIGATORIA);
    }

    [Fact]
    public void Validate_WithTooShortPassword_ShouldHaveMinLengthPasswordError()
    {
        var command = new CreateUserCommand("João", "joao@example.com", "52998224725", "short");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_SENHA_TAMANHO_MINIMO);
    }
}
