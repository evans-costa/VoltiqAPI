using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.ValueObjects;

public sealed class Address
{
    public string Street { get; private set; } = null!;
    public string Number { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;

    private Address() { }

    private Address(string street, string number, string city, string state, string zipCode)
    {
        Street = street;
        Number = number;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Create(string? street, string? number, string? city, string? state, string? zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException(ResourceErrorMessages.CLIENTE_ENDERECO_LOGRADOURO_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException(ResourceErrorMessages.CLIENTE_ENDERECO_NUMERO_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException(ResourceErrorMessages.CLIENTE_ENDERECO_CIDADE_OBRIGATORIA);

        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException(ResourceErrorMessages.CLIENTE_ENDERECO_ESTADO_OBRIGATORIO);

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new DomainException(ResourceErrorMessages.CLIENTE_ENDERECO_CEP_OBRIGATORIO);

        return new Address(street.Trim(), number.Trim(), city.Trim(), state.Trim(), zipCode.Trim());
    }
}
