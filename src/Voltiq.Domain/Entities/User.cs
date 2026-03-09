using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public class User : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; }
    public Document Document { get; private set; }
    public string PasswordHash { get; private set; } = null!;

    private User() { }

    private User(string name, Email email, Document document, string passwordHash)
    {
        Name = name;
        Email = email;
        Document = document;
        PasswordHash = passwordHash;
        AddDomainEvent(new UserCreatedEvent(Id));
    }

    public static User Create(string name, Email email, Document document, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.NOME_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(ResourceErrorMessages.HASH_SENHA_OBRIGATORIO);

        return new User(name.Trim(), email, document, passwordHash);
    }
}
