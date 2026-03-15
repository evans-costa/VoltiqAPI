using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;

namespace Voltiq.Domain.Tests.Entities;

public class UserTests
{
    private const string VALID_NAME = "João Silva";
    private const string VALID_PASSWORD_HASH = "$argon2id$hashed";

    private static Email ValidEmail() => Email.Create("joao@example.com").Value;
    private static Document ValidDocument() => Document.Create("529.982.247-25").Value;

    [Fact]
    public void Register_WithValidData_ShouldRegisterUser()
    {
        var user = User.Register(VALID_NAME, ValidEmail(), ValidDocument(), VALID_PASSWORD_HASH);

        user.Id.ShouldNotBe(Guid.Empty);
        user.Name.ShouldBe(VALID_NAME);
        user.Email.Value.ShouldBe("joao@example.com");
        user.Document.Value.ShouldNotBeNullOrWhiteSpace();
        user.PasswordHash.ShouldBe(VALID_PASSWORD_HASH);
    }

    [Fact]
    public void Create_ShouldRaise_UserRegisteredEvent()
    {
        var user = User.Register(VALID_NAME, ValidEmail(), ValidDocument(), VALID_PASSWORD_HASH);

        user.DomainEvents.ShouldContain(e => e is UserRegisteredEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
            User.Register(name!, ValidEmail(), ValidDocument(), VALID_PASSWORD_HASH));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyPasswordHash_ShouldThrowDomainException(string? hash)
    {
        Should.Throw<DomainException>(() =>
            User.Register(VALID_NAME, ValidEmail(), ValidDocument(), hash!));
    }
}
