using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Clients.Commands.UpdateClient;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Clients.Commands;

public class UpdateClientCommandHandlerTests
{
    private readonly Mock<IClientReadOnlyRepository> _clientReadRepoMock = new();
    private readonly Mock<IClientUpdateOnlyRepository> _clientUpdateRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private UpdateClientCommandHandler CreateHandler()
    {
        return new UpdateClientCommandHandler(_clientReadRepoMock.Object, _clientUpdateRepoMock.Object, _unitOfWorkMock.Object);
    }

    private static Client MakeClient(Guid userId)
    {
        var address = Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100");
        var email = Email.Create("joao@example.com").Value;
        return Client.Register(userId, "João Silva", "(11) 99999-9999", email, address);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateClientAndReturnUpdated()
    {
        var client = MakeClient(_userId);
        _clientUpdateRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(client.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientReadRepoMock
            .Setup(r => r.ExistsWithEmailForUserAsync(It.IsAny<Email>(), _userId, client.Id, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateClientCommand(
            client.Id, "Maria Souza", "(11) 88888-8888", "maria@example.com",
            "Av. Paulista", "1000", "São Paulo", "SP", "01311-100") { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExistsForAnotherClient_ShouldReturnConflictError()
    {
        var client = MakeClient(_userId);
        _clientUpdateRepoMock
            .Setup(r => r.GetByIdAndUserIdAsync(client.Id, _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientReadRepoMock
            .Setup(r => r.ExistsWithEmailForUserAsync(It.IsAny<Email>(), _userId, client.Id, It
                .IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateClientCommand(
            client.Id, "Maria Souza", "(11) 88888-8888", "outro@example.com",
            "Av. Paulista", "1000", "São Paulo", "SP", "01311-100") { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_EMAIL_JA_CADASTRADO);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNotFoundError()
    {
        _clientUpdateRepoMock
            .Setup(r =>
                r.GetByIdAndUserIdAsync(It.IsAny<Guid>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var command = new UpdateClientCommand(
            Guid.NewGuid(), "Maria Souza", "(11) 88888-8888", "maria@example.com",
            "Av. Paulista", "1000", "São Paulo", "SP", "01311-100") { UserId = _userId };

        var handler = CreateHandler();
        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_NAO_ENCONTRADO);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
