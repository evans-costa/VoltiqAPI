namespace Voltiq.Application.Features.Clients.Commands.RegisterClient;

public sealed record RegisterClientRequest(
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode);
