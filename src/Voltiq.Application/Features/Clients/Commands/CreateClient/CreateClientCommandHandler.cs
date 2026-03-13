using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Application.Features.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateClientCommand, ErrorOr<ClientResponse>>
{
    public Task<ErrorOr<ClientResponse>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
