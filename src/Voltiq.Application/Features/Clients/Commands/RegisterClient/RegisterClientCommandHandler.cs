using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.RegisterClient;

public sealed class RegisterClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RegisterClientCommand, ErrorOr<ClientResponse>>
{
    public async Task<ErrorOr<ClientResponse>> Handle(RegisterClientCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var email = Email.Create(request.Email).Value;

        var emailExists = await clientRepository.ExistsWithEmailForUserAsync(
            email, userId, cancellationToken: cancellationToken);

        if (emailExists)
            return Error.Conflict(description: ResourceErrorMessages.CLIENTE_EMAIL_JA_CADASTRADO);

        var address = Address.Create(request.Street, request.Number, request.City, request.State,
            request.ZipCode);
        var client = Client.Register(userId, request.Name, request.Phone, email, address);

        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.ToResponse();
    }
}
