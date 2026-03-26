using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class UserRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private Repository<User> _repository = null!;
    private UnitOfWork _unitOfWork = null!;
    private UserRepository _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext =
            ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), Guid.Empty);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _repository = new Repository<User>(_dbContext);
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
        var email = Email.Create("joao@example.com").Value;
        var document = Document.Create("529.982.247-25").Value;
        var user = User.Register("João Silva", email, document, "$argon2id$hash");

        await _repository.AddAsync(user, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _repository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(user.Id);
        found.Name.ShouldBe("João Silva");
        found.Email.Value.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task ExistsUserAsync_ShouldReturnTrue_WhenEmailOrDocumentExists()
    {
        var email = Email.Create("maria@example.com").Value;
        var document = Document.Create("11222333000181").Value;
        var user = User.Register("Maria Santos", email, document, "$argon2id$hash");

        await _repository.AddAsync(user, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var exists =
            await _userRepository.ExistsUserAsync(document, email,
                TestContext.Current.CancellationToken);

        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        var email = Email.Create("carlos@example.com").Value;
        var document = Document.Create("153.509.460-56").Value;
        var user = User.Register("Carlos Souza", email, document, "$argon2id$hash");

        await _repository.AddAsync(user, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found =
            await _userRepository.GetByEmailAsync(email, TestContext.Current.CancellationToken);

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
