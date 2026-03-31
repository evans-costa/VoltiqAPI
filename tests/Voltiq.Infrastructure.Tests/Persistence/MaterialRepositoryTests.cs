using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Repositories;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Infrastructure.Persistence.Repositories.Material;
using Voltiq.Infrastructure.Persistence.Repositories.User;

namespace Voltiq.Infrastructure.Tests.Persistence;

public class MaterialRepositoryTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>, IAsyncLifetime
{
    private static readonly Guid UserId = Guid.NewGuid();

    private MaterialRepository _materialRepository = null!;
    private IMaterialReadOnlyRepository _materialReadOnly = null!;
    private ApplicationDbContext _dbContext = null!;
    private UnitOfWork _unitOfWork = null!;
    private UserRepository _userRepository = null!;

    public async ValueTask InitializeAsync()
    {
        _dbContext = ApplicationDbContextFactory.Create(fixture.Container.GetConnectionString(), UserId);
        await _dbContext.Database.MigrateAsync();
        await DatabaseHelper.CleanAsync(_dbContext);

        _userRepository = new UserRepository(_dbContext);
        _materialRepository = new MaterialRepository(_dbContext);
        _materialReadOnly = _materialRepository;
        _unitOfWork = new UnitOfWork(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    private static User MakeUser(string email = "joao@example.com", string doc = "529.982.247-25")
    {
        var e = Email.Create(email).Value;
        var d = Document.Create(doc).Value;
        return User.Register("João Silva", e, d, "$argon2id$hash");
    }

    private static Material MakeMaterial(Guid userId, string name = "Cabo 10mm")
        => Material.Register(userId, name, 15.50m, MaterialUnit.Metro);

    private async Task<User> CreateAndSaveUserAsync(string email = "joao@example.com", string doc = "529.982.247-25")
    {
        var user = MakeUser(email, doc);
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddAndGetById_ShouldPersistMaterial()
    {
        var user = await CreateAndSaveUserAsync();
        var material = MakeMaterial(user.Id);

        await _materialRepository.AddAsync(material, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _materialRepository.GetByIdAsync(material.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(material.Id);
        found.UserId.ShouldBe(user.Id);
        found.Name.ShouldBe("Cabo 10mm");
        found.DefaultPrice.ShouldBe(15.50m);
        found.Unit.ShouldBe(MaterialUnit.Metro);
        found.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyMaterialsOfUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("maria@example.com", "11222333000181");

        var m1 = MakeMaterial(user1.Id, "Cabo A");
        var m2 = MakeMaterial(user1.Id, "Cabo B");
        var m3 = MakeMaterial(user2.Id, "Disjuntor");

        await _materialRepository.AddAsync(m1, TestContext.Current.CancellationToken);
        await _materialRepository.AddAsync(m2, TestContext.Current.CancellationToken);
        await _materialRepository.AddAsync(m3, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var user1Materials = await _materialRepository.GetByUserIdAsync(user1.Id, TestContext.Current.CancellationToken);

        user1Materials.Count.ShouldBe(2);
        user1Materials.ShouldAllBe(m => m.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnMaterial_WhenBelongsToUser()
    {
        var user = await CreateAndSaveUserAsync();
        var material = MakeMaterial(user.Id);

        await _materialRepository.AddAsync(material, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _materialReadOnly.GetByIdAndUserIdAsync(material.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(material.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenMaterialBelongsToAnotherUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("outro@example.com", "11222333000181");

        var material = MakeMaterial(user1.Id);
        await _materialRepository.AddAsync(material, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var found = await _materialReadOnly.GetByIdAndUserIdAsync(material.Id, user2.Id, TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnOnlyActiveMaterials()
    {
        var user = await CreateAndSaveUserAsync();

        var active = MakeMaterial(user.Id, "Ativo");
        var inactive = MakeMaterial(user.Id, "Inativo");
        inactive.Deactivate();

        await _materialRepository.AddAsync(active, TestContext.Current.CancellationToken);
        await _materialRepository.AddAsync(inactive, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _materialRepository.GetActiveByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Ativo");
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldNotReturnMaterialsOfOtherUser()
    {
        var user1 = await CreateAndSaveUserAsync();
        var user2 = await CreateAndSaveUserAsync("outro@example.com", "11222333000181");

        await _materialRepository.AddAsync(MakeMaterial(user1.Id, "User1 Material"), TestContext.Current.CancellationToken);
        await _materialRepository.AddAsync(MakeMaterial(user2.Id, "User2 Material"), TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _materialRepository.GetActiveByUserIdAsync(user1.Id, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result.ShouldAllBe(m => m.UserId == user1.Id);
    }
}
