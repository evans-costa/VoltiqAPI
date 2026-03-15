using System.Text.RegularExpressions;
using ErrorOr;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

public readonly partial record struct Email
{
    public string Value { get; }

    public Email(string value) => Value = value;

    public static ErrorOr<Email> Create(string? raw)
    {
        return TryParse(raw, out var email, out var errorMessage)
            ? email
            : Error.Validation(description: errorMessage);
    }

    public static bool TryParse(string? raw, out Email email, out string errorMessage)
    {
        email = default;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            errorMessage = ResourceErrorMessages.EMAIL_OBRIGATORIO;
            return false;
        }

        var normalised = raw.Trim().ToLowerInvariant();

        if (!EmailFormat().IsMatch(normalised))
        {
            errorMessage = ResourceErrorMessages.EMAIL_INVALIDO;
            return false;
        }

        email = new Email(normalised);
        return true;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailFormat();
}
