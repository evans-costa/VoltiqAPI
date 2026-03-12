using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Auth.Commands.Refresh;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;
using Voltiq.Exceptions.Errors;

namespace Voltiq.Application.Tests.Features.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private RefreshTokenCommandHandler CreateHandler() =>
        new(_refreshTokenRepoMock.Object, _userRepoMock.Object,
            _tokenServiceMock.Object, _unitOfWorkMock.Object);

    private static User MakeUser()
    {
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        return User.Create("João Silva", email, document, "$argon2id$hashed");
    }

    [Fact]
    public async Task Handle_WhenTokenNotFound_ReturnsUnauthorizedFailure()
    {
        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await CreateHandler().Handle(new("invalid-token"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ReturnsUnauthorizedFailure()
    {
        var expiredToken = RefreshToken.Create("expired-token", Guid.NewGuid(), expiresInDays: -1);

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var result = await CreateHandler().Handle(new("expired-token"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task Handle_WhenTokenRevoked_ReturnsUnauthorizedFailure()
    {
        var revokedToken = RefreshToken.Create("revoked-token", Guid.NewGuid(), expiresInDays: 7);
        revokedToken.Revoke();

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        var result = await CreateHandler().Handle(new("revoked-token"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.ShouldBeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task Handle_WhenTokenValid_RevokesOldToken()
    {
        var user = MakeUser();
        var activeToken = RefreshToken.Create("active-token", user.Id, expiresInDays: 7);

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns("new-access");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");

        await CreateHandler().Handle(new("active-token"), CancellationToken.None);

        activeToken.IsRevoked.ShouldBeTrue();
        activeToken.IsActive.ShouldBeFalse();
        activeToken.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenTokenValid_ReturnsNewAccessAndRefreshTokens()
    {
        var user = MakeUser();
        var activeToken = RefreshToken.Create("active-token", user.Id, expiresInDays: 7);

        _refreshTokenRepoMock
            .Setup(r => r.GetByTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeToken);
        _userRepoMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns("new-access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh-token");

        var result = await CreateHandler().Handle(new("active-token"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeOfType<AuthResponse>();
        result.Value.AccessToken.ShouldBe("new-access-token");
        result.Value.RefreshToken.ShouldBe("new-refresh-token");
    }
}
