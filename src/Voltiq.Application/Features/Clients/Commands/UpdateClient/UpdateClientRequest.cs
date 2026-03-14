namespace Voltiq.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientRequest(
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode);
