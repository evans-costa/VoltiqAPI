using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Application.Features.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(
    IClientRepository clientRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetClientByIdQuery, ErrorOr<ClientResponse>>
{
    public Task<ErrorOr<ClientResponse>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
