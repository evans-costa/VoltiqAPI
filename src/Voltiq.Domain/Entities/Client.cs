using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Client : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public Email Email { get; private set; }
    public Address Address { get; private set; } = null!;
    public Guid UserId { get; private set; }

    private Client() { }

    private Client(Guid userId, string name, string phone, Email email, Address address)
    {
        UserId = userId;
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        AddDomainEvent(new ClientRegisteredEvent(Id));
    }

    public static Client Register(Guid userId, string name, string phone, Email email, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);

        return new Client(userId, name.Trim(), phone.Trim(), email, address);
    }

    public void Update(string name, string phone, Email email, Address address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);

        Name = name.Trim();
        Phone = phone.Trim();
        Email = email;
        Address = address;
    }
}
