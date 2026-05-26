#:sdk Microsoft.NET.Sdk
#:property TargetFramework=net10.0
#:property Nullable=enable
#:property ImplicitUsings=enable
#:property PublishAot=false
#:project ../src/Voltiq.Infrastructure/Voltiq.Infrastructure.csproj
#:project ../src/Voltiq.Domain/Voltiq.Domain.csproj
#:project ../src/Voltiq.Application/Voltiq.Application.csproj

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Enums;
using Voltiq.Domain.ValueObjects;
using Voltiq.Infrastructure.Auth;
using Voltiq.Infrastructure.Persistence;
using Voltiq.Infrastructure.Persistence.Interceptors;

// ─── Configuração ────────────────────────────────────────────────────────────

const string SEED_EMAIL = "dev@voltiq.dev";
const string SEED_PASSWORD = "senha@123";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../src/Voltiq.Api"))
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile("appsettings.Development.json", true)
    .AddEnvironmentVariables()
    .Build();

var connectionString =
    configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=VoltiqDb;Port=5433;Username=postgres;Password=postgres";

// ─── Stub de serviços ────────────────────────────────────────────────────────

var currentUserService = new SeedCurrentUserService();

var interceptor = new SoftDeleteInterceptor();

var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(connectionString)
    .AddInterceptors(interceptor)
    .Options;

await using var db = new ApplicationDbContext(dbOptions, currentUserService);

// ─── Idempotência ─────────────────────────────────────────────────────────────

var email = Email.Create(SEED_EMAIL).Value;
var existingUser = await db.Users
    .AsNoTracking()
    .FirstOrDefaultAsync(u => u.Email == email);

if (existingUser is not null)
{
    Console.WriteLine(
        $@"⚠️  Seed já aplicado (usuário '{SEED_EMAIL}' já existe). Nenhum dado foi inserido.");
    return;
}

Console.WriteLine(@"🌱 Iniciando seed de desenvolvimento...");

// ─── Usuário ─────────────────────────────────────────────────────────────────

var hasher = new Argon2PasswordHasher();
var passwordHash = hasher.Hash(SEED_PASSWORD);

var userDocument = Document.Create("529.982.247-25").Value;
var user = User.Register("Dev Voltiq", email, userDocument, passwordHash);

await db.Users.AddAsync(user);
await db.SaveChangesAsync();

currentUserService.SetUserId(user.Id);

Console.WriteLine($@"✅ Usuário criado: {user.Name} <{SEED_EMAIL}>");

// ─── Clientes ─────────────────────────────────────────────────────────────────

var clients = new[]
{
    Client.Register(user.Id, "Construtora ABC Ltda", "(11) 3333-4444",
        Email.Create("contato@construtorabc.com.br").Value,
        Address.Create("Av. Paulista", "1000", "São Paulo", "SP", "01310-100")),

    Client.Register(user.Id, "João da Silva", "(21) 99999-1234",
        Email.Create("joao.silva@gmail.com").Value,
        Address.Create("Rua das Flores", "45", "Rio de Janeiro", "RJ", "20040-020")),

    Client.Register(user.Id, "Empresa XYZ S.A.", "(51) 3200-5678",
        Email.Create("financeiro@xyz.com.br").Value,
        Address.Create("Rua dos Andradas", "800", "Porto Alegre", "RS", "90020-004"))
};

await db.Clients.AddRangeAsync(clients);
await db.SaveChangesAsync();

Console.WriteLine($@"✅ {clients.Length} clientes criados.");

// ─── Materiais ────────────────────────────────────────────────────────────────

var materials = new[]
{
    Material.Register(user.Id, "Cabo Flexível 2,5mm", 4.80m, MaterialUnit.Metro),
    Material.Register(user.Id, "Cabo Flexível 4mm", 7.20m, MaterialUnit.Metro),
    Material.Register(user.Id, "Tomada 2P+T", 18.50m, MaterialUnit.Unidade),
    Material.Register(user.Id, "Disjuntor 20A", 32.00m, MaterialUnit.Unidade)
};

await db.Materials.AddRangeAsync(materials);
await db.SaveChangesAsync();

Console.WriteLine($@"✅ {materials.Length} materiais criados.");

// ─── Orçamentos ───────────────────────────────────────────────────────────────

// Orçamento 1: Construtora ABC — mix de material vinculado + item customizado
var budget1 = Budget.Register(user.Id, clients[0].Id);
budget1.AddItem(BudgetItem.Create(budget1.Id, materials[0].Id, BudgetItemType.Material,
    MaterialUnit.Metro, 50, 4.80m, "Cabo Flexível 2,5mm"));
budget1.AddItem(BudgetItem.Create(budget1.Id, materials[2].Id, BudgetItemType.Material, MaterialUnit.Unidade,
    8, 18.50m, "Tomada 2P+T"));
budget1.AddItem(BudgetItem.Create(budget1.Id, null, BudgetItemType.MaoDeObra, null, 1, 350.00m, "Mão de obra elétrica"));

await db.Budgets.AddAsync(budget1);
await db.SaveChangesAsync();

// Orçamento 2: João da Silva — item customizado
var budget2 = Budget.Register(user.Id, clients[1].Id);
budget2.AddItem(BudgetItem.Create(budget2.Id, materials[3].Id, BudgetItemType.Material,
    MaterialUnit.Unidade, 2, 32.00m, "Disjuntor 20A"));

await db.Budgets.AddAsync(budget2);
await db.SaveChangesAsync();

Console.WriteLine(@"✅ 2 orçamentos criados.");
Console.WriteLine();
Console.WriteLine(@"🎉 Seed concluído! Acesse com:");
Console.WriteLine($@"   E-mail:  {SEED_EMAIL}");
Console.WriteLine($@"   Senha:   {SEED_PASSWORD}");

// ─── Stub ─────────────────────────────────────────────────────────────────────

internal sealed class SeedCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; private set; } = Guid.Empty;

    public string UserName => "seed";
    public bool IsAuthenticated => true;

    public void SetUserId(Guid id)
    {
        UserId = id;
    }
}
