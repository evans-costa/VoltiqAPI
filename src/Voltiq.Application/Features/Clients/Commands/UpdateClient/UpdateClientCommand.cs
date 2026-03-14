using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string Phone,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode) : IRequest<ErrorOr<Updated>>;
