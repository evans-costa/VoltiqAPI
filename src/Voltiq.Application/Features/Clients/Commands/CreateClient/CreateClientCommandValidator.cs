using FluentValidation;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.CreateClient;

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_ENDERECO_LOGRADOURO_OBRIGATORIO);

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_ENDERECO_NUMERO_OBRIGATORIO);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_ENDERECO_CIDADE_OBRIGATORIA);

        RuleFor(x => x.State)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_ENDERECO_ESTADO_OBRIGATORIO);

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage(ResourceErrorMessages.CLIENTE_ENDERECO_CEP_OBRIGATORIO);
    }
}
