using Microsoft.EntityFrameworkCore;
using Shouldly;
using Voltiq.CommonTestUtilities.Builders;
using Voltiq.CommonTestUtilities.Database;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Domain.Enums;
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

    [Fact]
    public async Task AddAndGetById_ShouldPersistMaterial()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);
        var material = await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user.Id,
            cancellationToken: TestContext.Current.CancellationToken);

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
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "maria@example.com", document: "11222333000181",
            cancellationToken: TestContext.Current.CancellationToken);

        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user1.Id, name: "Cabo A",
            cancellationToken: TestContext.Current.CancellationToken);
        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user1.Id, name: "Cabo B",
            cancellationToken: TestContext.Current.CancellationToken);
        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user2.Id, name: "Disjuntor",
            cancellationToken: TestContext.Current.CancellationToken);

        var user1Materials = await _materialRepository.GetByUserIdAsync(user1.Id, TestContext.Current.CancellationToken);

        user1Materials.Count.ShouldBe(2);
        user1Materials.ShouldAllBe(m => m.UserId == user1.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnMaterial_WhenBelongsToUser()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);
        var material = await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        var found = await _materialReadOnly.GetByIdAndUserIdAsync(material.Id, user.Id, TestContext.Current.CancellationToken);

        found.ShouldNotBeNull();
        found.Id.ShouldBe(material.Id);
    }

    [Fact]
    public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenMaterialBelongsToAnotherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "outro@example.com", document: "11222333000181",
            cancellationToken: TestContext.Current.CancellationToken);

        var material = await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user1.Id,
            cancellationToken: TestContext.Current.CancellationToken);

        var found = await _materialReadOnly.GetByIdAndUserIdAsync(material.Id, user2.Id, TestContext.Current.CancellationToken);

        found.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnOnlyActiveMaterials()
    {
        var user = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);

        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user.Id, name: "Ativo",
            cancellationToken: TestContext.Current.CancellationToken);

        var inactive = TestDataBuilder.MakeMaterial(user.Id, name: "Inativo");
        inactive.Deactivate();
        await _materialRepository.AddAsync(inactive, TestContext.Current.CancellationToken);
        await _unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _materialRepository.GetActiveByUserIdAsync(user.Id, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Ativo");
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldNotReturnMaterialsOfOtherUser()
    {
        var user1 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            cancellationToken: TestContext.Current.CancellationToken);
        var user2 = await TestDataBuilder.SeedUserAsync(_userRepository, _unitOfWork,
            email: "outro@example.com", document: "11222333000181",
            cancellationToken: TestContext.Current.CancellationToken);

        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user1.Id, name: "User1 Material",
            cancellationToken: TestContext.Current.CancellationToken);
        await TestDataBuilder.SeedMaterialAsync(_materialRepository, _unitOfWork, user2.Id, name: "User2 Material",
            cancellationToken: TestContext.Current.CancellationToken);

        var result = await _materialRepository.GetActiveByUserIdAsync(user1.Id, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result.ShouldAllBe(m => m.UserId == user1.Id);
    }
}
