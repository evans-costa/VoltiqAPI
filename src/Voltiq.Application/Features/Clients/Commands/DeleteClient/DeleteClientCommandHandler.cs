using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;

namespace Voltiq.Application.Features.Clients.Commands.DeleteClient;

public sealed class DeleteClientCommandHandler(
    IClientRepository clientRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeleteClientCommand, ErrorOr<Deleted>>
{
    public Task<ErrorOr<Deleted>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
