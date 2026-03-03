using System.Text.RegularExpressions;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    private static readonly Regex _format =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException(ResourceErrorMessages.EMAIL_OBRIGATORIO);

        var normalised = raw.Trim().ToLowerInvariant();

        if (!_format.IsMatch(normalised))
            throw new DomainException(string.Format(ResourceErrorMessages.EMAIL_INVALIDO, raw));

        return new Email(normalised);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
