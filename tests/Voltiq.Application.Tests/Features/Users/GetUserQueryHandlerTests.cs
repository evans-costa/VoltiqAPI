using Moq;
using Shouldly;
using Voltiq.Application.Features.Users.Queries.GetUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Exceptions.Exceptions;

namespace Voltiq.Application.Tests.Features.Users;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private GetUserQueryHandler CreateHandler() =>
        new(_userRepoMock.Object);

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnUserResponse()
    {
        var user = User.Create("João Silva", "joao@example.com", "529.982.247-25", "$argon2id$hash");

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetUserQuery(user.Id), CancellationToken.None);

        result.Name.ShouldBe("João Silva");
        result.Email.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldThrowNotFoundException()
    {
        var id = Guid.NewGuid();

        _userRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        await Should.ThrowAsync<NotFoundException>(
            () => handler.Handle(new GetUserQuery(id), CancellationToken.None));
    }
}
