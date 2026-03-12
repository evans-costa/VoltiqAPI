using FluentValidation.TestHelper;
using Voltiq.Application.Features.Auth.Commands.Refresh;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Auth;

public class RefreshTokenCommandValidatorTests
{
    private readonly RefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidRefreshToken_ShouldNotHaveErrors()
    {
        var command = new RefreshTokenCommand("valid-refresh-token");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyOrNullRefreshToken_ShouldHaveInvalidMessage(string? token)
    {
        var command = new RefreshTokenCommand(token!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage(ResourceErrorMessages.REFRESH_TOKEN_INVALIDO);
    }
}
