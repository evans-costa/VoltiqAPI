using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Events;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Tests.Entities;

public class ClientTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    private static Address ValidAddress()
    {
        return Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100");
    }

    private static Email ValidEmail()
    {
        return Email.Create("joao@example.com").Value;
    }

    [Fact]
    public void Register_WithValidData_ShouldRegisterClient()
    {
        var client = Client.Register(ValidUserId, "João Silva", "(11) 99999-9999", ValidEmail(),
            ValidAddress());

        client.Id.ShouldNotBe(Guid.Empty);
        client.UserId.ShouldBe(ValidUserId);
        client.Name.ShouldBe("João Silva");
        client.Phone.ShouldBe("(11) 99999-9999");
        client.Email.Value.ShouldBe("joao@example.com");
        client.Address.ShouldNotBeNull();
        client.Address.Street.ShouldBe("Rua das Flores");
        client.Address.Number.ShouldBe("123");
        client.Address.City.ShouldBe("São Paulo");
        client.Address.State.ShouldBe("SP");
        client.Address.ZipCode.ShouldBe("01310-100");
    }

    [Fact]
    public void Create_ShouldRaise_ClientRegisteredEvent()
    {
        var client = Client.Register(ValidUserId, "João Silva", "(11) 99999-9999", ValidEmail(),
            ValidAddress());

        client.DomainEvents.ShouldContain(e => e is ClientRegisteredEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
                Client.Register(ValidUserId, name!, "(11) 99999-9999", ValidEmail(),
                    ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyPhone_ShouldThrowDomainException(string? phone)
    {
        Should.Throw<DomainException>(() =>
                Client.Register(ValidUserId, "João Silva", phone!, ValidEmail(), ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);
    }

    [Fact]
    public void Register_TrimsName()
    {
        var client = Client.Register(ValidUserId, "  João Silva  ", "(11) 99999-9999", ValidEmail(),
            ValidAddress());

        client.Name.ShouldBe("João Silva");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var client = Client.Register(ValidUserId, "João Silva", "(11) 99999-9999", ValidEmail(),
            ValidAddress());
        var newAddress = Address.Create("Av. Paulista", "1000", "São Paulo", "SP", "01311-100");
        var newEmail = Email.Create("maria@example.com").Value;

        client.Update("Maria Souza", "(11) 88888-8888", newEmail, newAddress);

        client.Name.ShouldBe("Maria Souza");
        client.Phone.ShouldBe("(11) 88888-8888");
        client.Email.Value.ShouldBe("maria@example.com");
        client.Address.Street.ShouldBe("Av. Paulista");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        var client = Client.Register(ValidUserId, "João Silva", "(11) 99999-9999", ValidEmail(),
            ValidAddress());

        Should.Throw<DomainException>(() =>
                client.Update(name!, "(11) 99999-9999", ValidEmail(), ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithNullOrEmptyPhone_ShouldThrowDomainException(string? phone)
    {
        var client = Client.Register(ValidUserId, "João Silva", "(11) 99999-9999", ValidEmail(),
            ValidAddress());

        Should.Throw<DomainException>(() =>
                client.Update("João Silva", phone!, ValidEmail(), ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);
    }
}
