using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid Id) : IAuthenticatedRequest<ErrorOr<ClientResponse>>
{
    public Guid UserId { get; set; }
}
