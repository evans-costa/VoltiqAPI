using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Tests.Entities;

public class ServiceTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Register_WithValidData_ShouldRegisterService()
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica Residencial", 150.00m);

        service.Id.ShouldNotBe(Guid.Empty);
        service.UserId.ShouldBe(ValidUserId);
        service.Name.ShouldBe("Instalação Elétrica Residencial");
        service.BasePrice.ShouldBe(150.00m);
        service.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Register_ShouldRaise_ServiceRegisteredEvent()
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica Residencial", 150.00m);

        service.DomainEvents.ShouldContain(e => e is ServiceRegisteredEvent);
        var domainEvent = (ServiceRegisteredEvent)service.DomainEvents.First(e => e is ServiceRegisteredEvent);
        domainEvent.ServiceId.ShouldBe(service.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
            Service.Register(ValidUserId, name!, 100m))
            .Message.ShouldBe(ResourceErrorMessages.SERVICE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Register_WithInvalidPrice_ShouldThrowDomainException(decimal price)
    {
        Should.Throw<DomainException>(() =>
            Service.Register(ValidUserId, "Instalação", price))
            .Message.ShouldBe(ResourceErrorMessages.SERVICE_PRECO_INVALIDO);
    }

    [Fact]
    public void Register_TrimsName()
    {
        var service = Service.Register(ValidUserId, "  Instalação Elétrica  ", 100m);

        service.Name.ShouldBe("Instalação Elétrica");
    }

    [Fact]
    public void Register_WithEmptyUserId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            Service.Register(Guid.Empty, "Instalação", 100m))
            .Message.ShouldBe(ResourceErrorMessages.SERVICE_USUARIO_OBRIGATORIO);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var service = Service.Register(ValidUserId, "Instalação", 100m);

        service.Deactivate();

        service.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var service = Service.Register(ValidUserId, "Instalação", 100m);
        service.Deactivate();

        service.Activate();

        service.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica", 150.00m);

        service.Update("Manutenção Residencial", 120.00m);

        service.Name.ShouldBe("Manutenção Residencial");
        service.BasePrice.ShouldBe(120.00m);
    }

    [Fact]
    public void Update_ShouldTrimName()
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica", 150.00m);

        service.Update("  Manutenção Residencial  ", 120.00m);

        service.Name.ShouldBe("Manutenção Residencial");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica", 150.00m);

        Should.Throw<DomainException>(() => service.Update(name!, 120.00m))
            .Message.ShouldBe(ResourceErrorMessages.SERVICE_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Update_WithInvalidPrice_ShouldThrowDomainException(decimal price)
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica", 150.00m);

        Should.Throw<DomainException>(() => service.Update("Manutenção Residencial", price))
            .Message.ShouldBe(ResourceErrorMessages.SERVICE_PRECO_INVALIDO);
    }

    [Fact]
    public void Update_ShouldNotAffectIsActive()
    {
        var service = Service.Register(ValidUserId, "Instalação Elétrica", 150.00m);
        service.Deactivate();

        service.Update("Manutenção Residencial", 120.00m);

        service.IsActive.ShouldBeFalse();
    }
}
