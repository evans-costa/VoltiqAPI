using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Users;

public class GetCurrentUserQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private GetCurrentUserQueryHandler CreateHandler() =>
        new(_currentUserServiceMock.Object, _userRepoMock.Object);

    private static User MakeUser()
    {
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        return User.Register("João Silva", email, document, "$argon2id$hash");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnCurrentUser()
    {
        var user = MakeUser();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Email.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ShouldReturnNotFoundError()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.Empty);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(
            string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), Guid.Empty));
        _userRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundInDb_ShouldReturnNotFoundError()
    {
        var id = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(id);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        result.FirstError.Description.ShouldBe(
            string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), id));
    }
}
