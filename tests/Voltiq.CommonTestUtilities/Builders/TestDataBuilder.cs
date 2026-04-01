using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.Interfaces;
using Voltiq.Domain.Interfaces.Repositories.Budget;
using Voltiq.Domain.Interfaces.Repositories.Client;
using Voltiq.Domain.Interfaces.Repositories.Material;
using Voltiq.Domain.Interfaces.Repositories.RefreshToken;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Domain.ValueObjects;

namespace Voltiq.CommonTestUtilities.Builders;

public static class TestDataBuilder
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static User MakeUser(
        string name = "João Silva",
        string email = "joao@example.com",
        string document = "529.982.247-25",
        string passwordHash = "$argon2id$hash")
    {
        var emailVo = Email.Create(email).Value;
        var documentVo = Document.Create(document).Value;
        return User.Register(name, emailVo, documentVo, passwordHash);
    }

    public static Client MakeClient(
        Guid userId,
        string name = "Cliente Teste",
        string email = "cliente@example.com",
        string phone = "(11) 99999-9999")
    {
        var emailVo = Email.Create(email).Value;
        return Client.Register(userId, name, phone, emailVo,
            Address.Create("Rua das Flores", "123", "São Paulo", "SP", "01310-100"));
    }

    public static Material MakeMaterial(
        Guid userId,
        string name = "Cabo 10mm",
        decimal defaultPrice = 15.50m,
        MaterialUnit unit = MaterialUnit.Metro)
    {
        return Material.Register(userId, name, defaultPrice, unit);
    }

    public static Budget MakeBudget(Guid userId, Guid clientId)
    {
        return Budget.Register(userId, clientId);
    }

    public static RefreshToken MakeRefreshToken(
        string token,
        Guid userId,
        int daysToExpire = 7)
    {
        return RefreshToken.Create(token, userId, daysToExpire);
    }


    public static async Task<User> SeedUserAsync(
        IUserWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        string name = "João Silva",
        string email = "joao@example.com",
        string document = "529.982.247-25")
    {
        var user = MakeUser(name, email, document);
        await repository.AddAsync(user, Ct);
        await unitOfWork.SaveChangesAsync(Ct);
        return user;
    }

    public static async Task<Client> SeedClientAsync(
        IClientWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        Guid userId,
        string name = "Cliente Teste",
        string email = "cliente@example.com")
    {
        var client = MakeClient(userId, name, email);
        await repository.AddAsync(client, Ct);
        await unitOfWork.SaveChangesAsync(Ct);
        return client;
    }

    public static async Task<Material> SeedMaterialAsync(
        IMaterialWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        Guid userId,
        string name = "Cabo 10mm",
        decimal defaultPrice = 15.50m,
        MaterialUnit unit = MaterialUnit.Metro)
    {
        var material = MakeMaterial(userId, name, defaultPrice, unit);
        await repository.AddAsync(material, Ct);
        await unitOfWork.SaveChangesAsync(Ct);
        return material;
    }

    public static async Task<Budget> SeedBudgetAsync(
        IBudgetWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        Guid userId,
        Guid clientId)
    {
        var budget = MakeBudget(userId, clientId);
        await repository.AddAsync(budget, Ct);
        await unitOfWork.SaveChangesAsync(Ct);
        return budget;
    }

    public static async Task<RefreshToken> SeedRefreshTokenAsync(
        IRefreshTokenWriteOnlyRepository repository,
        IUnitOfWork unitOfWork,
        Guid userId,
        string token = "test-token",
        int daysToExpire = 7)
    {
        var refreshToken = MakeRefreshToken(token, userId, daysToExpire);
        await repository.AddAsync(refreshToken, Ct);
        await unitOfWork.SaveChangesAsync(Ct);
        return refreshToken;
    }
}
