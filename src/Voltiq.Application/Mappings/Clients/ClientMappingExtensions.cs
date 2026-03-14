using Voltiq.Application.Features.Clients;
using Voltiq.Application.Features.Clients.Commands.CreateClient;
using Voltiq.Application.Features.Clients.Commands.UpdateClient;
using Voltiq.Domain.Entities;

namespace Voltiq.Application.Mappings.Clients;

public static class ClientMappingExtensions
{
    extension(CreateClientRequest request)
    {
        public CreateClientCommand ToCommand() =>
            new(request.Name, request.Phone, request.Street, request.Number,
                request.City, request.State, request.ZipCode);
    }

    extension(UpdateClientRequest request)
    {
        public UpdateClientCommand ToCommand(Guid id) =>
            new(id, request.Name, request.Phone, request.Street, request.Number,
                request.City, request.State, request.ZipCode);
    }

    extension(Client client)
    {
        public ClientResponse ToResponse() =>
            new(client.Id, client.Name, client.Phone,
                client.Address.Street, client.Address.Number,
                client.Address.City, client.Address.State, client.Address.ZipCode);
    }
}
