using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Clients.Queries.GetClients;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Application.Tests.Features.Clients.Queries;

public class GetClientsQueryHandlerTests
{
    private readonly Mock<IClientReadOnlyRepository> _clientRepoMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetClientsQueryHandler CreateHandler() =>
        new(_clientRepoMock.Object);

    private static Client MakeClient(Guid userId, string name = "João Silva") =>
        Client.Register(userId, name, "(11) 99999-9999", Email.Create("joao@example.com").Value,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));

    [Fact]
    public async Task Handle_ShouldReturnClientsForCurrentUser()
    {
        var clients = new List<Client>
        {
            MakeClient(_userId, "João Silva"),
            MakeClient(_userId, "Maria Santos"),
        };
        _clientRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientsQuery { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(c => c.Name == "João Silva");
        result.Value.ShouldContain(c => c.Name == "Maria Santos");
    }

    [Fact]
    public async Task Handle_WhenNoClients_ShouldReturnEmptyList()
    {
        _clientRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientsQuery { UserId = _userId }, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }
}
