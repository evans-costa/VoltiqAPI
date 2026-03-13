using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Application.Features.Clients.Queries.GetClients;

public sealed class GetClientsQueryHandler(
    IClientRepository clientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetClientsQuery, ErrorOr<IReadOnlyList<ClientResponse>>>
{
    public Task<ErrorOr<IReadOnlyList<ClientResponse>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
