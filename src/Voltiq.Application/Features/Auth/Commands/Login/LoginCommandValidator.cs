using FluentValidation;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO)
            .Must(email => Email.TryParse(email, out _, out _))
            .WithMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_SENHA_OBRIGATORIA);
    }
}
