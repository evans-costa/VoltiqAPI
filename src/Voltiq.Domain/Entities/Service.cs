using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Service : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; }

    private Service() { }

    private Service(Guid userId, string name, decimal basePrice)
    {
        UserId = userId;
        Name = name;
        BasePrice = basePrice;
        IsActive = true;
        AddDomainEvent(new ServiceRegisteredEvent(Id));
    }

    public static Service Register(Guid userId, string name, decimal basePrice)
    {
        if (userId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.SERVICE_USUARIO_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.SERVICE_NOME_OBRIGATORIO);

        if (basePrice <= 0)
            throw new DomainException(ResourceErrorMessages.SERVICE_PRECO_INVALIDO);

        return new Service(userId, name.Trim(), basePrice);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void Update(string name, decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.SERVICE_NOME_OBRIGATORIO);

        if (basePrice <= 0)
            throw new DomainException(ResourceErrorMessages.SERVICE_PRECO_INVALIDO);

        Name = name.Trim();
        BasePrice = basePrice;
    }
}
