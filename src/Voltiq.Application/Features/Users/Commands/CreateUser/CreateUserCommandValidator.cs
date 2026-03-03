using FluentValidation;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.Usuario_Nome_Obrigatorio);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResourceErrorMessages.Usuario_Email_Obrigatorio)
            .EmailAddress().WithMessage(ResourceErrorMessages.Usuario_Email_Invalido);

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage(ResourceErrorMessages.Usuario_Documento_Obrigatorio)
            .Must(BeValidDocument).WithMessage(ResourceErrorMessages.Usuario_Documento_Invalido);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ResourceErrorMessages.Usuario_Senha_Obrigatoria)
            .MinimumLength(8).WithMessage(ResourceErrorMessages.Usuario_Senha_TamanhoMinimo);
    }

    private static bool BeValidDocument(string? document)
    {
        if (string.IsNullOrWhiteSpace(document)) return false;
        try
        {
            Domain.ValueObjects.Document.Create(document);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
