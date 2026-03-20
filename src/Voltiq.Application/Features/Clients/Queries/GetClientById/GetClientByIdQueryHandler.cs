using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(
    IClientRepository clientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetClientByIdQuery, ErrorOr<ClientResponse>>
{
    public async Task<ErrorOr<ClientResponse>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var client = await clientRepository.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        return client.ToResponse();
    }
}

