using System.Text.RegularExpressions;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

/// <summary>
/// Value object representing either a CPF (11 digits) or CNPJ (14 digits).
/// Stored as digits only; validation includes check-digit verification.
/// </summary>
public readonly partial record struct Document
{
    public string Value { get; } 

    private Document(string value) => Value = value;

    public static Document Create(string? raw)
    {
        return !TryParse(raw, out var document, out var errorMessage) ? 
            throw new DomainException(errorMessage) : document;
    }

    public static bool TryParse(string? raw, out Document document, out string errorMessage)
    {
        document = default;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            errorMessage = ResourceErrorMessages.DOCUMENTO_OBRIGATORIO;
            return false;
        }
        
        var digits = OnlyDigits().Replace(raw.Trim(), "");

        var isValid = digits.Length switch
        {
            11 => ValidateCpf(digits),
            14 => ValidateCnpj(digits),
            _ => false
        };
        
        if (!isValid)
        {
            errorMessage = ResourceErrorMessages.DOCUMENTO_FORMATO_INVALIDO;
            return false;
        }

        document = new Document(digits);
        return true;
    }

    private static bool ValidateCpf(string d)
    {
        if (d.Distinct().Count() == 1) return false;

        var sum1 = 0;
        for (var i = 0; i < 9; i++) sum1 += (d[i] - '0') * (10 - i);
        if (Digit(sum1) != (d[9] - '0')) return false;

        var sum2 = 0;
        for (var i = 0; i < 10; i++) sum2 += (d[i] - '0') * (11 - i);
        return Digit(sum2) == (d[10] - '0');

        static int Digit(int sum) { var r = sum % 11; return r < 2 ? 0 : 11 - r; }
    }

    private static bool ValidateCnpj(string d)
    {
        if (d.All(c => c == d[0])) return false;
        
        ReadOnlySpan<int> peso1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        ReadOnlySpan<int> peso2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var soma1 = 0;
        for (var i = 0; i < 12; i++)
            soma1 += (d[i] - '0') * peso1[i];

        if (Digit(soma1) != (d[12] - '0')) return false;

        var soma2 = 0;
        for (var i = 0; i < 13; i++)
            soma2 += (d[i] - '0') * peso2[i];

        return Digit(soma2) == (d[13] - '0');

        static int Digit(int sum) { var r = sum % 11; return r < 2 ? 0 : 11 - r; }
    }

    public override string ToString() => Value;
    [GeneratedRegex(@"\D", RegexOptions.Compiled)]
    private static partial Regex OnlyDigits();
}
