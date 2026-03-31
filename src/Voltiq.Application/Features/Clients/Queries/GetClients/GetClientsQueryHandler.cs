using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Application.Features.Clients.Queries.GetClients;

public sealed class GetClientsQueryHandler(IClientReadOnlyRepository clientRepository)
    : IRequestHandler<GetClientsQuery, ErrorOr<IReadOnlyList<ClientResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<ClientResponse>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var clients = await clientRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return clients.Select(c => c.ToResponse()).ToList();
    }
}
