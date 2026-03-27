using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateClientCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateClientCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var client =
            await clientRepository.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        var email = Email.Create(request.Email).Value;

        var emailExists = await clientRepository.ExistsWithEmailForUserAsync(
            email, userId, request.Id, cancellationToken);

        if (emailExists)
            return Error.Conflict(description: ResourceErrorMessages.CLIENTE_EMAIL_JA_CADASTRADO);

        var address = Address.Create(request.Street, request.Number, request.City, request.State,
            request.ZipCode);
        client.Update(request.Name, request.Phone, email, address);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
