using Shouldly;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("User.Name+tag@sub.domain.org")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldSucceed(string raw)
    {
        var email = Email.Create(raw);
        email.Value.ShouldBe(raw.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ShouldThrowDomainException(string? raw)
    {
        Should.Throw<DomainException>(() => Email.Create(raw!));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("two@@at.com")]
    public void Create_WithInvalidFormat_ShouldThrowDomainException(string raw)
    {
        Should.Throw<DomainException>(() => Email.Create(raw));
    }

    [Fact]
    public void TwoEmails_WithSameValue_ShouldBeEqual()
    {
        var a = Email.Create("test@example.com");
        var b = Email.Create("TEST@EXAMPLE.COM");
        a.ShouldBe(b);
    }
}
