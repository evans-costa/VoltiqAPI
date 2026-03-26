using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.TokenRepository;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class RefreshTokenRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private RefreshTokenRepository _repository = null!;
    private UnitOfWork _unitOfWork = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext = ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), Guid.Empty);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _repository = new RefreshTokenRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private async Task<(User user, RefreshToken token)> SeedUserAndTokenAsync(string rawToken)
    {
        var email = Email.Create($"{Guid.NewGuid()}@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        var user = User.Register("Test User", email, document, "$argon2id$hash");

        await _dbContext.Users.AddAsync(user);

        var token = RefreshToken.Create(rawToken, user.Id, 7);
        await _repository.AddAsync(token);
        await _unitOfWork.SaveChangesAsync();

        return (user, token);
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenExists_ReturnsToken()
    {
        var (_, expected) = await SeedUserAndTokenAsync("my-raw-token-abc");

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
