using ErrorOr;
using MediatR;
using Moq;
using Shouldly;
using Voltiq.Application.Common.Behaviors;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Tests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Guid _userId = Guid.NewGuid();

    private AuthorizationBehavior<TestAuthRequest, ErrorOr<string>> CreateBehavior() =>
        new(_currentUserServiceMock.Object);

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldPopulateUserIdAndCallNext()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(_userId);
        var request = new TestAuthRequest();
        var nextCalled = false;

        var behavior = CreateBehavior();
        var result = await behavior.Handle(
            request,
            ct =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("ok");
            },
            CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("ok");
        nextCalled.ShouldBeTrue();
        request.UserId.ShouldBe(_userId);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsEmpty_ShouldReturnUnauthorizedWithoutCallingNext()
    {
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.Empty);
        var request = new TestAuthRequest();
        var nextCalled = false;

        var behavior = CreateBehavior();
        var result = await behavior.Handle(
            request,
            ct =>
            {
                nextCalled = true;
                return Task.FromResult<ErrorOr<string>>("ok");
            },
            CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
        result.FirstError.Description.ShouldBe(ResourceErrorMessages.TITULO_NAO_AUTORIZADO);
        nextCalled.ShouldBeFalse();
    }

    private sealed class TestAuthRequest : IAuthenticatedRequest<ErrorOr<string>>
    {
        public Guid UserId { get; set; }
    }
}
