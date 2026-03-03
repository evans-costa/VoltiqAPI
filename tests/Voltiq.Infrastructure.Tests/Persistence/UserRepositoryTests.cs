using Microsoft.EntityFrameworkCore;
using Shouldly;
using Testcontainers.PostgreSql;
using Voltiq.Domain.Entities;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .Build();

    private ApplicationDbContext _dbContext = null!;
    private Repository<User> _repository = null!;
    private UnitOfWork _unitOfWork = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _repository = new Repository<User>(_dbContext);
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistUser()
    {
        var user = User.Create("João Silva", "joao@example.com", "529.982.247-25", "$argon2id$hash");

        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var found = await _repository.GetByIdAsync(user.Id);

        found.ShouldNotBeNull();
        found!.Id.ShouldBe(user.Id);
        found.Name.ShouldBe("João Silva");
        found.Email.Value.ShouldBe("joao@example.com");
    }

    [Fact]
    public async Task FindByEmail_ShouldReturnMatchingUser()
    {
        var user = User.Create("Maria Santos", "maria@example.com", "11222333000181", "$argon2id$hash");

        await _repository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var results = await _repository.FindAsync(u => u.Email.Value == "maria@example.com");

        results.ShouldHaveSingleItem();
        results[0].Name.ShouldBe("Maria Santos");
    }
}
