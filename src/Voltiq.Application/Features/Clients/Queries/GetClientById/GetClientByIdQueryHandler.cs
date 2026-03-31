using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientReadOnlyRepository clientRepository)
    : IRequestHandler<GetClientByIdQuery, ErrorOr<ClientResponse>>
{
    public async Task<ErrorOr<ClientResponse>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAndUserIdAsync(request.Id, request.UserId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        return client.ToResponse();
    }
}
