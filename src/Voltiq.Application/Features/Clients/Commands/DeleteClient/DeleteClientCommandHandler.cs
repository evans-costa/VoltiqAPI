using ErrorOr;
using MediatR;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientUpdateOnlyRepository clientRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteClientCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteClientCommand request,
        CancellationToken cancellationToken)
    {
        var client =
            await clientRepository.GetByIdAndUserIdAsync(request.Id, request.UserId, cancellationToken);

        if (client is null)
            return Error.NotFound(description: ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);

        clientRepository.Remove(client);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}
