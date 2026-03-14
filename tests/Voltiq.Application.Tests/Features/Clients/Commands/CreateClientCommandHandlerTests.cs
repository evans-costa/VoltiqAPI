using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Clients;
using Voltiq.Application.Features.Clients.Commands.CreateClient;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Clients.Commands;

public class CreateClientCommandHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    private readonly Guid _userId = Guid.NewGuid();

    private CreateClientCommandHandler CreateHandler() =>
        new(_clientRepoMock.Object, _unitOfWorkMock.Object, _currentUserServiceMock.Object);

    private static CreateClientCommand ValidCommand() =>
        new("João Silva", "(11) 99999-9999", "Rua das Flores", "123", "São Paulo", "SP", "01310-100");

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateClientAndReturnResponse()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId.ToString());

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Phone.ShouldBe("(11) 99999-9999");
        result.Value.Street.ShouldBe("Rua das Flores");
        result.Value.City.ShouldBe("São Paulo");
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsInvalid_ShouldReturnUnauthorizedError()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
        _clientRepoMock.Verify(r => r.AddAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
