using FluentValidation.TestHelper;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new LoginCommand("joao@example.com", "senha123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveEmailError()
    {
        var command = new LoginCommand("", "senha123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO);
    }

    [Fact]
    public void Validate_WithInvalidEmailFormat_ShouldHaveEmailError()
    {
        var command = new LoginCommand("nao-e-email", "senha123");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHavePasswordError()
    {
        var command = new LoginCommand("joao@example.com", "");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage(ResourceErrorMessages.USUARIO_SENHA_OBRIGATORIA);
    }
}
