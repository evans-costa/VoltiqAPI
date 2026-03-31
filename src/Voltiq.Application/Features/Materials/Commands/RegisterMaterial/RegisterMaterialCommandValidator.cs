using FluentValidation;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Materials.Commands.RegisterMaterial;

public sealed class RegisterMaterialCommandValidator : AbstractValidator<RegisterMaterialCommand>
{
    public RegisterMaterialCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);

        RuleFor(x => x.DefaultPrice)
            .GreaterThan(0).WithMessage(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);

        RuleFor(x => x.Unit)
            .IsInEnum().WithMessage(ResourceErrorMessages.MATERIAL_UNIDADE_INVALIDA);
    }
}
