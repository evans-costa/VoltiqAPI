using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Queries.GetClients;

public sealed class GetClientsQueryHandler(
    IClientRepository clientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetClientsQuery, ErrorOr<IReadOnlyList<ClientResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<ClientResponse>>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var clients = await clientRepository.GetByUserIdAsync(userId, cancellationToken);

        return clients.Select(c => c.ToResponse()).ToList();
    }
}

