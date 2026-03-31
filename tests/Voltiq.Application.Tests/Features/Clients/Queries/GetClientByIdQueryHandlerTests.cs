using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Clients.Queries.GetClientById;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Clients.Queries;

public class GetClientByIdQueryHandlerTests
{
    private readonly Mock<IClientReadOnlyRepository> _clientRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetClientByIdQueryHandler CreateHandler() =>
        new(_clientRepoMock.Object);

    private static Client MakeClient(Guid userId)
    {
        var address = Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100");
        var email = Email.Create("joao@example.com").Value;
        return Client.Register(userId, "João Silva", "(11) 99999-9999", email, address);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldReturnClientResponse()
    {
        var client = MakeClient(_userId);
        _clientRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(client.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientByIdQuery(client.Id) { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(client.Id);
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Phone.ShouldBe("(11) 99999-9999");
        result.Value.Email.ShouldBe("joao@example.com");
        result.Value.Street.ShouldBe("Rua das Flores");
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNotFoundError()
    {
        _clientRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientByIdQuery(Guid.NewGuid()) { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);
    }
}
