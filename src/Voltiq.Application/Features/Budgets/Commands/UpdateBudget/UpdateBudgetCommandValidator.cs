using FluentValidation;
using Voltiq.Domain.Enums;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Budgets.Commands.UpdateBudget;

public sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
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

            item.RuleFor(i => i.Type)
                .IsInEnum()
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_TIPO_INVALIDO);

            item.RuleFor(i => i.MaterialId)
                .NotNull()
                .When(i => i.Type == BudgetItemType.Material)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_MATERIAL_ID_OBRIGATORIO_PARA_MATERIAL);

            item.RuleFor(i => i.MaterialId)
                .Null()
                .When(i => i.Type != BudgetItemType.Material && Enum.IsDefined(i.Type))
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_MATERIAL_ID_DEVE_SER_NULO);

            item.RuleFor(i => i.Unit)
                .NotNull()
                .When(i => i.Type == BudgetItemType.Material)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_UNIDADE_OBRIGATORIA_PARA_MATERIAL);

            item.RuleFor(i => i.Unit)
                .Null()
                .When(i => i.Type != BudgetItemType.Material && Enum.IsDefined(i.Type))
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_UNIDADE_DEVE_SER_NULA);

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_QUANTIDADE_INVALIDA);

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0)
                .WithMessage(ResourceErrorMessages.ORCAMENTO_ITEM_PRECO_INVALIDO);
        });
    }
}
