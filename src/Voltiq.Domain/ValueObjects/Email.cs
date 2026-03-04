using System.Text.RegularExpressions;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

public readonly partial record struct Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException(ResourceErrorMessages.EMAIL_OBRIGATORIO);

        var normalised = raw.Trim().ToLowerInvariant();

        return !EmailFormat().IsMatch(normalised) ?
            throw new DomainException(string.Format(ResourceErrorMessages.EMAIL_INVALIDO, raw)) :
            new Email(normalised);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "pt-BR")]
    private static partial Regex EmailFormat();
}
