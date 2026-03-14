using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Commands.RegisterClient;

public sealed record RegisterClientCommand(
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode) : IRequest<ErrorOr<ClientResponse>>;
