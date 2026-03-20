using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Testcontainers.PostgreSql;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.TokenRepository;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class RefreshTokenRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private ApplicationDbContext _dbContext = null!;
    private RefreshTokenRepository _repository = null!;
    private UnitOfWork _unitOfWork = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(s => s.UserId).Returns(Guid.Empty);

        _dbContext = new ApplicationDbContext(options, currentUser.Object);
        await _dbContext.Database.MigrateAsync();

        _repository = new RefreshTokenRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
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

        var found = await _repository.GetByTokenAsync("my-raw-token-abc");

        found.ShouldNotBeNull();
        found.Id.ShouldBe(expected.Id);
        found.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByTokenAsync_WhenTokenNotFound_ReturnsNull()
    {
        var found = await _repository.GetByTokenAsync("token-que-nao-existe");

        found.ShouldBeNull();
    }
}
