namespace Voltiq.Application.Features.Clients;

public sealed record ClientResponse(
    Guid Id,
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode);
