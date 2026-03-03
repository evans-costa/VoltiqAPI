using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Entities;

public class User : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public Document Document { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    // Required by EF Core
    private User() { }

    private User(string name, Email email, Document document, string passwordHash)
    {
        Name = name;
        Email = email;
        Document = document;
        PasswordHash = passwordHash;
        AddDomainEvent(new UserCreatedEvent(Id));
    }

    public static User Create(string name, string email, string document, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ResourceErrorMessages.Nome_Obrigatorio);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(ResourceErrorMessages.HashSenha_Obrigatorio);

        return new User(
            name.Trim(),
            Email.Create(email),
            Document.Create(document),
            passwordHash);
    }
}
