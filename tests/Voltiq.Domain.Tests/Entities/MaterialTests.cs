using Shouldly;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Tests.Entities;

public class MaterialTests
{
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Register_WithValidData_ShouldRegisterMaterial()
    {
        var material = Material.Register(ValidUserId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);

        material.Id.ShouldNotBe(Guid.Empty);
        material.UserId.ShouldBe(ValidUserId);
        material.Name.ShouldBe("Cabo 10mm");
        material.DefaultPrice.ShouldBe(15.50m);
        material.Unit.ShouldBe(MaterialUnit.Metro);
        material.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Register_ShouldRaise_MaterialRegisteredEvent()
    {
        var material = Material.Register(ValidUserId, "Cabo 10mm", 15.50m, MaterialUnit.Metro);

        material.DomainEvents.ShouldContain(e => e is MaterialRegisteredEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Register_WithNullOrEmptyName_ShouldThrowDomainException(string? name)
    {
        Should.Throw<DomainException>(() =>
            Material.Register(ValidUserId, name!, 10m, MaterialUnit.Unidade))
            .Message.ShouldBe(ResourceErrorMessages.MATERIAL_NOME_OBRIGATORIO);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Register_WithInvalidPrice_ShouldThrowDomainException(decimal price)
    {
        Should.Throw<DomainException>(() =>
            Material.Register(ValidUserId, "Cabo", price, MaterialUnit.Unidade))
            .Message.ShouldBe(ResourceErrorMessages.MATERIAL_PRECO_INVALIDO);
    }

    [Fact]
    public void Register_TrimsName()
    {
        var material = Material.Register(ValidUserId, "  Cabo 10mm  ", 10m, MaterialUnit.Metro);

        material.Name.ShouldBe("Cabo 10mm");
    }

    [Fact]
    public void Register_WithEmptyUserId_ShouldThrowDomainException()
    {
        Should.Throw<DomainException>(() =>
            Material.Register(Guid.Empty, "Cabo 10mm", 10m, MaterialUnit.Unidade))
            .Message.ShouldBe(ResourceErrorMessages.MATERIAL_USUARIO_OBRIGATORIO);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var material = Material.Register(ValidUserId, "Cabo", 10m, MaterialUnit.Unidade);

        material.Deactivate();

        material.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var material = Material.Register(ValidUserId, "Cabo", 10m, MaterialUnit.Unidade);
        material.Deactivate();

        material.Activate();

        material.IsActive.ShouldBeTrue();
    }
}
