using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Clients.Commands.RegisterClient;

public sealed record RegisterClientCommand(
    string Name,
    string Phone,
    string Email,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode) : IAuthenticatedRequest<ErrorOr<ClientResponse>>
{
    public Guid UserId { get; set; }
}
