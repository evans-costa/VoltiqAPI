using Voltiq.Domain.Enums;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Material : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal DefaultPrice { get; private set; }
    public MaterialUnit Unit { get; private set; }
    public bool IsActive { get; private set; }

    private Material() { }

    private Material(Guid userId, string name, decimal defaultPrice, MaterialUnit unit)
    {
        UserId = userId;
        Name = name;
        DefaultPrice = defaultPrice;
        Unit = unit;
        IsActive = true;
        AddDomainEvent(new MaterialRegisteredEvent(Id));
    }

    public static Material Register(Guid userId, string name, decimal defaultPrice, MaterialUnit unit)
    {
        if (userId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.MATERIAL_USUARIO_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);

        if (!Enum.IsDefined(unit))
            throw new DomainException(ResourceErrorMessages.MATERIAL_UNIDADE_INVALIDA);

        if (defaultPrice <= 0)
            throw new DomainException(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);

        return new Material(userId, name.Trim(), defaultPrice, unit);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void Update(string name, decimal defaultPrice, MaterialUnit unit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);

        if (!Enum.IsDefined(unit))
            throw new DomainException(ResourceErrorMessages.MATERIAL_UNIDADE_INVALIDA);

        if (defaultPrice <= 0)
            throw new DomainException(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);

        Name = name.Trim();
        DefaultPrice = defaultPrice;
        Unit = unit;
    }
}
