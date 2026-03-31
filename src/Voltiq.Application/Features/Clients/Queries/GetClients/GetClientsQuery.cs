using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Clients.Queries.GetClients;

public sealed record GetClientsQuery : IAuthenticatedRequest<ErrorOr<IReadOnlyList<ClientResponse>>>
{
    public Guid UserId { get; set; }
}
