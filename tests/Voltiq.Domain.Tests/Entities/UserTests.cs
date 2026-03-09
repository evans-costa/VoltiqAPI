using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;

namespace Voltiq.Domain.Tests.Entities;

public class UserTests
{
    private const string ValidName = "João Silva";
    private const string ValidPasswordHash = "$argon2id$hashed";

    private static Email ValidEmail() => Email.Create("joao@example.com").Value;
    private static Document ValidDocument() => Document.Create("529.982.247-25").Value;

    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var user = User.Create(ValidName, ValidEmail(), ValidDocument(), ValidPasswordHash);

        user.Id.ShouldNotBe(Guid.Empty);
        user.Name.ShouldBe(ValidName);
        user.Email.Value.ShouldBe("joao@example.com");
        user.Document.Value.ShouldNotBeNullOrWhiteSpace();
        user.PasswordHash.ShouldBe(ValidPasswordHash);
    }

    [Fact]
    public void Create_ShouldRaise_UserCreatedEvent()
    {
        var user = User.Create(ValidName, ValidEmail(), ValidDocument(), ValidPasswordHash);

        user.DomainEvents.ShouldContain(e => e is UserCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
            User.Create(name!, ValidEmail(), ValidDocument(), ValidPasswordHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyPasswordHash_ShouldThrowDomainException(string? hash)
    {
        Should.Throw<DomainException>(() =>
            User.Create(ValidName, ValidEmail(), ValidDocument(), hash!));
    }
}
