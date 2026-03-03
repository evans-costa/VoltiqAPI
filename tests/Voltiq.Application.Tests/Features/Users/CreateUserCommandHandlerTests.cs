using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Users.Commands.CreateUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;

namespace Voltiq.Application.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();

    private CreateUserCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _unitOfWorkMock.Object, _passwordHasherMock.Object);

    private static CreateUserCommand ValidCommand() =>
        new("João Silva", "joao@example.com", "529.982.247-25", "S3cur3P@ssw0rd!");

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithUserId()
    {
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("$argon2id$hashed");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        var existingUser = User.Create("Other", "joao@example.com", "529.982.247-25", "$argon2id$hash");
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([existingUser]);

        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("$argon2id$hash");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNullOrWhiteSpace();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDocumentAlreadyExists_ShouldReturnFailure()
    {
        // First call (email check) returns empty; second call (document check) returns existing
        var existingUser = User.Create("Other", "other@example.com", "529.982.247-25", "$argon2id$hash");
        var callCount = 0;
        _userRepoMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? [] : [existingUser]);

        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("$argon2id$hash");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNullOrWhiteSpace();
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
