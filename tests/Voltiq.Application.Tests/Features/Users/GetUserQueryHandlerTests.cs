using Moq;
using Shouldly;
using Voltiq.Application.Features.Users.Queries.GetUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Errors;

namespace Voltiq.Application.Tests.Features.Users;

public class GetUserQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private GetUserQueryHandler CreateHandler() =>
        new(_userRepoMock.Object);

    [Fact]
    public async Task Handle_WithExistingId_ShouldReturnUserResponse()
    {
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        var user = User.Create("João Silva", email, document, "$argon2id$hash");

        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetUserQuery(user.Id), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("João Silva");
        result.Value.Email.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnNotFoundError()
    {
        var id = Guid.NewGuid();

        _userRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(new GetUserQuery(id), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<NotFoundError>();
    }
}
