using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Application.Features.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateClientCommand, ErrorOr<ClientResponse>>
{
    public Task<ErrorOr<ClientResponse>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
