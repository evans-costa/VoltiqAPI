using FluentValidation;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_NOME_OBRIGATORIO);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO)
            .EmailAddress().WithMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_OBRIGATORIO)
            .Must(d => Document.TryParse(d, out _, out _))
            .WithMessage(ResourceErrorMessages.USUARIO_DOCUMENTO_INVALIDO);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_SENHA_OBRIGATORIA)
            .MinimumLength(8).WithMessage(ResourceErrorMessages.USUARIO_SENHA_TAMANHO_MINIMO);
    }
}
