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

    private static Address ValidAddress() =>
        Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100");

    [Fact]
    public void Create_WithValidData_ShouldCreateClient()
    {
        var client = Client.Create(ValidUserId, "João Silva", "(11) 99999-9999", ValidAddress());

        client.Id.ShouldNotBe(Guid.Empty);
        client.UserId.ShouldBe(ValidUserId);
        client.Name.ShouldBe("João Silva");
        client.Phone.ShouldBe("(11) 99999-9999");
        client.Address.ShouldNotBeNull();
        client.Address.Street.ShouldBe("Rua das Flores");
        client.Address.Number.ShouldBe("123");
        client.Address.City.ShouldBe("São Paulo");
        client.Address.State.ShouldBe("SP");
        client.Address.ZipCode.ShouldBe("01310-100");
    }

    [Fact]
    public void Create_ShouldRaise_ClientCreatedEvent()
    {
        var client = Client.Create(ValidUserId, "João Silva", "(11) 99999-9999", ValidAddress());

        client.DomainEvents.ShouldContain(e => e is ClientCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
            Client.Create(ValidUserId, name!, "(11) 99999-9999", ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyPhone_ShouldThrowDomainException(string? phone)
    {
        Should.Throw<DomainException>(() =>
            Client.Create(ValidUserId, "João Silva", phone!, ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var client = Client.Create(ValidUserId, "  João Silva  ", "(11) 99999-9999", ValidAddress());

        client.Name.ShouldBe("João Silva");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var client = Client.Create(ValidUserId, "João Silva", "(11) 99999-9999", ValidAddress());
        var newAddress = Address.Create("Av. Paulista", "1000", "São Paulo", "SP", "01311-100");

        client.Update("Maria Souza", "(11) 88888-8888", newAddress);

        client.Name.ShouldBe("Maria Souza");
        client.Phone.ShouldBe("(11) 88888-8888");
        client.Address.Street.ShouldBe("Av. Paulista");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        var client = Client.Create(ValidUserId, "João Silva", "(11) 99999-9999", ValidAddress());

        Should.Throw<DomainException>(() =>
            client.Update(name!, "(11) 99999-9999", ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Update_WithNullOrEmptyPhone_ShouldThrowDomainException(string? phone)
    {
        var client = Client.Create(ValidUserId, "João Silva", "(11) 99999-9999", ValidAddress());

        Should.Throw<DomainException>(() =>
            client.Update("João Silva", phone!, ValidAddress()))
            .Message.ShouldBe(ResourceErrorMessages.CLIENTE_TELEFONE_OBRIGATORIO);
    }
}
