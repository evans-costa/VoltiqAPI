using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Clients;
using Voltiq.Application.Features.Clients.Queries.GetClients;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.Application.Tests.Features.Clients.Queries;

public class GetClientsQueryHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetClientsQueryHandler CreateHandler() =>
        new(_clientRepoMock.Object, _currentUserServiceMock.Object);

    private static Client MakeClient(Guid userId, string name = "João Silva") =>
        Client.Register(userId, name, "(11) 99999-9999",
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));

    [Fact]
    public async Task Handle_ShouldReturnClientsForCurrentUser()
    {
        var clients = new List<Client>
        {
            MakeClient(_userId, "João Silva"),
            MakeClient(_userId, "Maria Santos"),
        };
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId.ToString());
        _clientRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2);
        result.Value.ShouldContain(c => c.Name == "João Silva");
        result.Value.ShouldContain(c => c.Name == "Maria Santos");
    }

    [Fact]
    public async Task Handle_WhenNoClients_ShouldReturnEmptyList()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId.ToString());
        _clientRepoMock
            .Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserIdIsInvalid_ShouldReturnUnauthorizedError()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
    }
}
