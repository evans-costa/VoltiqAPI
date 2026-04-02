using FluentValidation;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.RegisterBudget;

public sealed class RegisterBudgetCommandValidator : AbstractValidator<RegisterBudgetCommand>
{
    public RegisterBudgetCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MaterialName)
                .NotEmpty()
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_NOME_OBRIGATORIO);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);
        });
    }
}
