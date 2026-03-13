using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Client : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private Client() { }

    private Client(Guid userId, string name, string phone, Address address)
    {
        UserId = userId;
        Name = name;
        Phone = phone;
        Address = address;
        AddDomainEvent(new ClientCreatedEvent(Id));
    }

    public static Client Create(Guid userId, string name, string phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);

        return new Client(userId, name.Trim(), phone.Trim(), address);
    }

    public void Update(string name, string phone, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);

        Name = name.Trim();
        Phone = phone.Trim();
        Address = address;
    }
}
