using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class UserRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private UserRepository _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext =
            ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), Guid.Empty);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _userRepository = new UserRepository(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork);

        var found = await _userRepository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(user.Id);
        found.Name.ShouldBe("João Silva");
        found.Email.Value.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task ExistsUserAsync_ShouldReturnTrue_WhenEmailOrDocumentExists()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            name: "Maria Santos", email: "maria@example.com", document: "11222333000181");

        var exists = await _userRepository.ExistsUserAsync(
            user.Document, user.Email, TestContext.Current.CancellationToken);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            name: "Carlos Souza", email: "carlos@example.com", document: "153.509.460-56");

        var found = await _userRepository.GetByEmailAsync(user.Email, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(user.Id);
        found.Name.ShouldBe("Carlos Souza");
        found.Email.Value.ShouldBe("carlos@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailNotFound()
    {
        var email = Email.Create("naoexiste@example.com").Value;

        var found =
            await _userRepository.GetByEmailAsync(email, TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }
}
