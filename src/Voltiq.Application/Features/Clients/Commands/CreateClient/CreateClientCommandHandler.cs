using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateClientCommand, ErrorOr<ClientResponse>>
{
    public async Task<ErrorOr<ClientResponse>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId) || userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var address = Address.Create(request.Street, request.Number, request.City, request.State, request.ZipCode);
        var client = Client.Create(userId, request.Name, request.Phone, address);

        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.ToResponse();
    }
}

