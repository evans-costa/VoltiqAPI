using FluentValidation;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(ResourceErrorMessages.REFRESH_TOKEN_INVALIDO);
    }
}
