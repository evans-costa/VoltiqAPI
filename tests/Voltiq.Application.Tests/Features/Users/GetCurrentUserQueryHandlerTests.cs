using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Errors;

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
        return User.Create("João Silva", email, document, "$argon2id$hash");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnCurrentUser()
    {
        var user = MakeUser();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(user.Id.ToString());
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Email.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldReturnNotFoundError()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<NotFoundError>();
        _userRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Handle_WhenUserIdIsInvalidGuid_ShouldReturnNotFoundError(string invalidId)
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(invalidId);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<NotFoundError>();
        _userRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundInDb_ShouldReturnNotFoundError()
    {
        var id = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(id.ToString());
        _userRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<NotFoundError>();
    }
}
