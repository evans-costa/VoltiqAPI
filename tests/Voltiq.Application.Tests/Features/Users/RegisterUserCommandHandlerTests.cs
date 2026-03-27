using ErrorOr;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Users.Commands.RegisterUser;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Users;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _unitOfWorkMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object, _refreshTokenRepoMock.Object);

    private static RegisterUserCommand ValidCommand() =>
        new("João Silva", "joao@example.com", "529.982.247-25", "S3cur3P@ssw0rd!");

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithUserIdAndTokens()
    {
        _userRepoMock
            .Setup(r => r.ExistsUserAsync(It.IsAny<Document>(), It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("$argon2id$hashed");

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns("jwt.token.here");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("refresh.token.here");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldNotBe(Guid.Empty);
        result.Value.AccessToken.ShouldBe("jwt.token.here");
        result.Value.RefreshToken.ShouldBe("refresh.token.here");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Once);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnConflictError()
    {
        _userRepoMock
            .Setup(r => r.ExistsUserAsync(It.IsAny<Document>(), It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("$argon2id$hash");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Conflict);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.USUARIO_EMAIL_JA_CADASTRADO);
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepoMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
        _tokenServiceMock.Verify(t => t.GenerateRefreshToken(), Times.Never);
    }
}
