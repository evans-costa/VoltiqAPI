using FluentValidation.TestHelper;
using Voltiq.Application.Features.Users.Commands.CreateUser;

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
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldHaveError(string? email)
    {
        var command = new CreateUserCommand("João", email!, "52998224725", "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("12345")]
    public void Validate_WithInvalidDocument_ShouldHaveError(string? document)
    {
        var command = new CreateUserCommand("João", "joao@example.com", document!, "S3cur3P@ss!");
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Document);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")]
    public void Validate_WithInvalidPassword_ShouldHaveError(string? password)
    {
        var command = new CreateUserCommand("João", "joao@example.com", "52998224725", password!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
