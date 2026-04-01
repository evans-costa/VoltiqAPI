using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.TokenRepository;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class RefreshTokenRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private RefreshTokenRepository _repository = null!;
    private UserRepository _userRepository = null!;
    private UnitOfWork _unitOfWork = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext = ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), Guid.Empty);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _userRepository = new UserRepository(_dbContext);
        _repository = new RefreshTokenRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenExists_ReturnsToken()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: $"{Guid.NewGuid()}@example.com");
        var expected = await TestDataBuilder.SeedRefreshTokenAsync(_repository, _unitOfWork,
            user.Id, token: "my-raw-token-abc");

        var found = await _repository.GetByTokenAsync("my-raw-token-abc",
            TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(expected.Id);
        found.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByTokenAsync("token-que-nao-existe",
            TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }
}
