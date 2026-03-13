namespace Voltiq.Application.Features.Clients.Commands.CreateClient;

public sealed record CreateClientRequest(
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode);
