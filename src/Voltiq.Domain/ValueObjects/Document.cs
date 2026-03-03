using System.Text.RegularExpressions;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

/// <summary>
/// Value object representing either a CPF (11 digits) or CNPJ (14 digits).
/// Stored as digits only; validation includes check-digit verification.
/// </summary>
public sealed class Document : ValueObject
{
    private static readonly Regex _digits = new(@"\D", RegexOptions.Compiled);

    public string Value { get; } // digits only

    private Document(string value) => Value = value;

    public static Document Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException(ResourceErrorMessages.Documento_Obrigatorio);

        var digits = _digits.Replace(raw.Trim(), "");

        return digits.Length switch
        {
            11 => ValidateCpf(digits),
            14 => ValidateCnpj(digits),
            _ => throw new DomainException(ResourceErrorMessages.Documento_FormatoInvalido)
        };
    }

    private static Document ValidateCpf(string d)
    {
        if (d.Distinct().Count() == 1)
            throw new DomainException(ResourceErrorMessages.Cpf_Invalido);

        int Sum(int len, int factor) =>
            Enumerable.Range(0, len).Sum(i => int.Parse(d[i].ToString()) * (factor - i));

        static int Digit(int sum) { var r = sum % 11; return r < 2 ? 0 : 11 - r; }

        if (Digit(Sum(9, 10)) != int.Parse(d[9].ToString()) ||
            Digit(Sum(10, 11)) != int.Parse(d[10].ToString()))
            throw new DomainException(ResourceErrorMessages.Cpf_Invalido);

        return new Document(d);
    }

    private static Document ValidateCnpj(string d)
    {
        if (d.Distinct().Count() == 1)
            throw new DomainException(ResourceErrorMessages.Cnpj_Invalido);

        static int Calc(string n, int[] weights) =>
            weights.Select((w, i) => int.Parse(n[i].ToString()) * w).Sum();

        static int Digit(int sum) { var r = sum % 11; return r < 2 ? 0 : 11 - r; }

        int[] w1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] w2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        if (Digit(Calc(d, w1)) != int.Parse(d[12].ToString()) ||
            Digit(Calc(d, w2)) != int.Parse(d[13].ToString()))
            throw new DomainException(ResourceErrorMessages.Cnpj_Invalido);

        return new Document(d);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
