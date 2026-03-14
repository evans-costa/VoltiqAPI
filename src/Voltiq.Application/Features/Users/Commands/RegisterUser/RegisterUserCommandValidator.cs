using FluentValidation;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_NOME_OBRIGATORIO);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO)
            .Must(email => Email.TryParse(email, out _, out _))
            .WithMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_OBRIGATORIO)
            .Must(doc => Document.TryParse(doc, out _, out _))
            .WithMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_INVALIDO);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_SENHA_OBRIGATORIA)
            .MinimumLength(8).WithMessage(ResourceErrorMessages.USUARIO_SENHA_TAMANHO_MINIMO);
    }
}
