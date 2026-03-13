using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode) : IRequest<ErrorOr<ClientResponse>>;
