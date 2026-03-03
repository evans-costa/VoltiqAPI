using Shouldly;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Tests.ValueObjects;

public class DocumentTests
{
    // CPF: 11 digits — valid formatted and unformatted
    [Theory]
    [InlineData("529.982.247-25")] // valid CPF with punctuation
    [InlineData("52998224725")]    // valid CPF digits only
    public void Create_WithValidCpf_ShouldSucceed(string raw)
    {
        var doc = Document.Create(raw);
        doc.Value.ShouldNotBeNullOrWhiteSpace();
    }

    // CNPJ: 14 digits — valid formatted and unformatted
    [Theory]
    [InlineData("11.222.333/0001-81")] // valid CNPJ with punctuation
    [InlineData("11222333000181")]      // valid CNPJ digits only
    public void Create_WithValidCnpj_ShouldSucceed(string raw)
    {
        var doc = Document.Create(raw);
        doc.Value.ShouldNotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldThrowDomainException(string? raw)
    {
        Should.Throw<DomainException>(() => Document.Create(raw!));
    }

    [Theory]
    [InlineData("111.111.111-11")] // all same digits — invalid CPF
    [InlineData("12345")]          // too short
    [InlineData("abc.def.ghi-jk")] // non-numeric
    [InlineData("000.000.000-00")] // all zeros — invalid
    public void Create_WithInvalidDocument_ShouldThrowDomainException(string raw)
    {
        Should.Throw<DomainException>(() => Document.Create(raw));
    }

    [Fact]
    public void TwoDocuments_WithSameDigits_ShouldBeEqual()
    {
        var a = Document.Create("529.982.247-25");
        var b = Document.Create("52998224725");
        a.ShouldBe(b);
    }
}
