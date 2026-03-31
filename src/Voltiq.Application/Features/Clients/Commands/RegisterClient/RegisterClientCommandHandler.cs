using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Clients;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Clients.Commands.RegisterClient;

public sealed class RegisterClientCommandHandler(
    IClientReadOnlyRepository clientReadOnlyRepository,
    IClientWriteOnlyRepository clientWriteOnlyRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterClientCommand, ErrorOr<ClientResponse>>
{
    public async Task<ErrorOr<ClientResponse>> Handle(RegisterClientCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;

        var emailExists = await clientReadOnlyRepository.ExistsWithEmailForUserAsync(
            email, request.UserId, cancellationToken: cancellationToken);

        if (emailExists)
            return Error.Conflict(description: ResourceErrorMessages.CLIENTE_EMAIL_JA_CADASTRADO);

        var address = Address.Create(request.Street, request.Number, request.City, request.State,
            request.ZipCode);
        var client = Client.Register(request.UserId, request.Name, request.Phone, email, address);

        await clientWriteOnlyRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return client.ToResponse();
    }
}
