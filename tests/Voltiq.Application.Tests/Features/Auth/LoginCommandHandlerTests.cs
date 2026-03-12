using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Auth.Commands.Login;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepoMock.Object, _passwordHasherMock.Object, _tokenServiceMock.Object,
            _refreshTokenRepoMock.Object, _unitOfWorkMock.Object);

    private static LoginCommand ValidCommand() =>
        new("joao@example.com", "S3cur3P@ssw0rd!");

    private static User MakeUser()
    {
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        return User.Create("João Silva", email, document, "$argon2id$hashed");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        var user = MakeUser();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns("jwt.token.here");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken())
            .Returns("refresh.token.here");

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("jwt.token.here");
        result.Value.RefreshToken.ShouldBe("refresh.token.here");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnUnauthorizedError()
    {
        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<UnauthorizedError>();
        result.FirstError.Message.ShouldBe(ResourceErrorMessages.LOGIN_CREDENCIAIS_INVALIDAS);
        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ShouldReturnUnauthorizedError()
    {
        var user = MakeUser();

        _userRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var handler = CreateHandler();
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<UnauthorizedError>();
        result.FirstError.Message.ShouldBe(ResourceErrorMessages.LOGIN_CREDENCIAIS_INVALIDAS);
        _tokenServiceMock.Verify(
            t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
    }
}
