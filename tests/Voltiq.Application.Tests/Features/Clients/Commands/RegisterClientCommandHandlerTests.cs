using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Features.Clients.Commands.RegisterClient;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Clients.Commands;

public class RegisterClientCommandHandlerTests
{
    private readonly Mock<IClientReadOnlyRepository> _clientReadRepoMock = new();
    private readonly Mock<IClientWriteOnlyRepository> _clientWriteRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private RegisterClientCommandHandler CreateHandler()
    {
        return new RegisterClientCommandHandler(_clientReadRepoMock.Object, _clientWriteRepoMock.Object, _unitOfWorkMock.Object);
    }

    private RegisterClientCommand ValidCommand() =>
        new("João Silva", "(11) 99999-9999", "joao@example.com",
            "Rua das Flores", "123",
            "São Paulo", "SP", "01310-100") { UserId = _userId };

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterClientAndReturnResponse()
    {
        _clientReadRepoMock
            .Setup(r => r.ExistsWithEmailForUserAsync(It.IsAny<Email>(), _userId, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Phone.ShouldBe("(11) 99999-9999");
        result.Value.Email.ShouldBe("joao@example.com");
        result.Value.Street.ShouldBe("Rua das Flores");
        result.Value.City.ShouldBe("São Paulo");
        _clientWriteRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExistsForUser_ShouldReturnConflictError()
    {
        _clientReadRepoMock
            .Setup(r => r.ExistsWithEmailForUserAsync(It.IsAny<Email>(), _userId, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.CLIENTE_EMAIL_JA_CADASTRADO);
        _clientWriteRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
