using Shouldly;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("User.Name+tag@sub.domain.org")]
    [InlineData("USER@EXAMPLE.COM")]
    public void Create_WithValidEmail_ShouldReturnSuccess(string raw)
    {
        var result = Email.Create(raw);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(raw.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespace_ShouldReturnFailure(string raw)
    {
        var result = Email.Create(raw);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    [InlineData("two@@at.com")]
    public void Create_WithInvalidFormat_ShouldReturnFailure(string raw)
    {
        var result = Email.Create(raw);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public void TwoEmails_WithSameValue_ShouldBeEqual()
    {
        var a = Email.Create("test@example.com").Value;
        var b = Email.Create("TEST@EXAMPLE.COM").Value;
        a.ShouldBe(b);
    }
}
