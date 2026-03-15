using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Material : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public decimal DefaultPrice { get; private set; }
    public string Unit { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Material() { }

    private Material(string name, decimal defaultPrice, string unit)
    {
        Name = name;
        DefaultPrice = defaultPrice;
        Unit = unit;
        IsActive = true;
        AddDomainEvent(new MaterialRegisteredEvent(Id));
    }

    public static Material Register(string name, decimal defaultPrice, string unit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException(ResourceErrorMessages.MATERIAL_UNIDADE_OBRIGATORIA);

        if (defaultPrice <= 0)
            throw new DomainException(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);

        return new Material(name.Trim(), defaultPrice, unit.Trim());
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
