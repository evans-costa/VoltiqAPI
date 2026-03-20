using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteClientCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteClientCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);

        var client =
            await clientRepository.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        clientRepository.Remove(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
