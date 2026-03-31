using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Street,
    string Number,
    string City,
    string State,
    string ZipCode) : IAuthenticatedRequest<ErrorOr<Updated>>
{
    public Guid UserId { get; set; }
}
