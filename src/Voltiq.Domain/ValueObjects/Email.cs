using System.Text.RegularExpressions;
using Voltiq.Domain.Common;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

public readonly partial record struct Email
{
    public string Value { get; }

    public Email(string value) => Value = value;

    public static Result<Email> Create(string? raw)
    {
        return TryParse(raw, out var email, out var errorMessage)
            ? Result<Email>.Success(email)
            : Result<Email>.Failure(new Error(errorMessage));
    }
    
    public static Email FromDatabase(string value)
    {
        TryParse(value, out var email, out _);
        return email;
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
