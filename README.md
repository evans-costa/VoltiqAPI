# Voltiq API

API RESTful construída com **.NET 10** seguindo os princípios de **Clean Architecture**, **CQRS** com MediatR e autenticação via **JWT**.

---

## Índice

- [Fluxo de Trabalho Git](#fluxo-de-trabalho-git)
- [Arquitetura](#arquitetura)
- [Estrutura de Pastas](#estrutura-de-pastas)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Comandos](#comandos)
- [Camadas](#camadas)
  - [Domain](#domain)
  - [Application](#application)
  - [Infrastructure](#infrastructure)
  - [API](#api)
- [Convenções](#convenções)
  - [CQRS com MediatR](#cqrs-com-mediatr)
  - [Entidades de Domínio](#entidades-de-domínio)
  - [Result Pattern](#result-pattern)
  - [Validação](#validação)
  - [Tratamento de Exceções](#tratamento-de-exceções)
  - [Repositório e Unit of Work](#repositório-e-unit-of-work)
  - [Autenticação JWT](#autenticação-jwt)
- [Testes](#testes)

---

## Fluxo de Trabalho Git

### Branches

Toda feature, correção ou melhoria é desenvolvida em uma branch dedicada, criada a partir de `main`:

| Tipo | Prefixo da branch | Exemplo |
|---|---|---|
| Nova funcionalidade | `feature/` | `feature/create-product` |
| Correção de bug | `fix/` | `fix/token-expiry-validation` |
| Hotfix urgente em produção | `hotfix/` | `hotfix/null-reference-login` |
| Refatoração | `refactor/` | `refactor/exception-handlers` |
| Documentação | `docs/` | `docs/update-readme` |
| Chores (configs, deps) | `chore/` | `chore/update-packages` |

```bash
# Criar e mudar para uma nova branch
git checkout -b feature/nome-da-feature

# Ao finalizar, abrir Pull Request para main
```

Nunca commite diretamente em `main`. Toda mudança entra via Pull Request com ao menos uma revisão.

### Conventional Commits

Todos os commits seguem o padrão [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope opcional>): <descrição curta em inglês>

[corpo opcional]

[rodapé opcional, ex: Breaking change, closes #issue]
```

| Tipo | Quando usar |
|---|---|
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug |
| `refactor` | Mudança de código que não é feat nem fix |
| `docs` | Alterações somente em documentação |
| `test` | Adição ou correção de testes |
| `chore` | Tarefas de manutenção (deps, config, CI) |
| `perf` | Melhoria de performance |
| `ci` | Mudanças em pipelines de CI/CD |

**Exemplos:**

```bash
feat(products): add create product command and handler
fix(auth): handle expired token on refresh
refactor(exceptions): replace middleware with IExceptionHandler chain
docs: add git workflow section to README
test(domain): add unit tests for Result pattern
chore: add .gitignore for .NET solution
```

---

## Arquitetura

O projeto segue **Clean Architecture** com regras estritas de dependência entre camadas:

```
Voltiq.API → Voltiq.Application + Voltiq.Infrastructure
Voltiq.Application → Voltiq.Domain
Voltiq.Infrastructure → Voltiq.Application + Voltiq.Domain
Voltiq.Domain → (sem dependências)
```

---

## Estrutura de Pastas

```
voltiq/
├── src/
│   ├── Voltiq.API/                        # Entrada da aplicação (controllers, handlers de exceção, DI)
│   │   ├── Controllers/
│   │   │   └── BaseApiController.cs       # Controller base com MediatR
│   │   ├── ExceptionHandlers/             # IExceptionHandler por tipo de exceção
│   │   │   ├── ValidationExceptionHandler.cs
│   │   │   ├── NotFoundExceptionHandler.cs
│   │   │   ├── UnauthorizedExceptionHandler.cs
│   │   │   └── GlobalExceptionHandler.cs
│   │   └── Program.cs
│   │
│   ├── Voltiq.Application/                # Casos de uso (CQRS, validators, interfaces)
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs  # Pipeline MediatR para validação
│   │   │   └── Interfaces/
│   │   │       ├── IApplicationDbContext.cs
│   │   │       ├── ICurrentUserService.cs
│   │   │       └── ITokenService.cs
│   │   ├── Features/                      # Commands e Queries por feature
│   │   └── DependencyInjection.cs
│   │
│   ├── Voltiq.Domain/                     # Núcleo do domínio (sem dependências externas)
│   │   ├── Common/
│   │   │   └── Result.cs                  # Result pattern (railway-oriented)
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs              # Id (GUID) + DomainEvents
│   │   │   └── AuditableEntity.cs         # + CreatedAt, CreatedBy, UpdatedAt
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs
│   │   │   └── BaseDomainEvent.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs         # Exceção base de domínio (400)
│   │   │   └── NotFoundException.cs       # Recurso não encontrado (404)
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── ValueObjects/
│   │       └── ValueObject.cs
│   │
│   └── Voltiq.Infrastructure/             # EF Core, JWT, repositórios
│       ├── Auth/
│       │   ├── TokenService.cs
│       │   └── CurrentUserService.cs
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   └── Repositories/
│       │       ├── Repository.cs
│       │       └── UnitOfWork.cs
│       └── DependencyInjection.cs
│
└── tests/
    ├── Voltiq.Domain.Tests/
    ├── Voltiq.Application.Tests/
    └── Voltiq.Infrastructure.Tests/
```

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local ou via Docker)

---

## Configuração

Edite `src/Voltiq.API/appsettings.json` (ou use variáveis de ambiente / User Secrets em desenvolvimento):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VoltiqDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "SUA_CHAVE_SECRETA_COM_32_CARACTERES_OU_MAIS",
    "Issuer": "Voltiq.API",
    "Audience": "Voltiq.Client",
    "ExpiresInMinutes": "60"
  }
}
```

| Chave | Descrição |
|---|---|
| `ConnectionStrings:DefaultConnection` | Connection string do SQL Server |
| `JwtSettings:SecretKey` | Chave de assinatura JWT — **mínimo 32 caracteres** |
| `JwtSettings:Issuer` | Emissor do token (padrão: `Voltiq.API`) |
| `JwtSettings:Audience` | Audiência do token (padrão: `Voltiq.Client`) |
| `JwtSettings:ExpiresInMinutes` | Tempo de expiração do token em minutos (padrão: `60`) |

---

## Comandos

```bash
# Compilar a solução
dotnet build Voltiq.slnx

# Executar todos os testes
dotnet test Voltiq.slnx

# Executar testes de um projeto específico
dotnet test tests/Voltiq.Domain.Tests
dotnet test tests/Voltiq.Application.Tests
dotnet test tests/Voltiq.Infrastructure.Tests

# Executar um teste específico
dotnet test tests/Voltiq.Domain.Tests --filter "FullyQualifiedName~NomeDaClasse.NomeDoMetodo"

# Executar a API
dotnet run --project src/Voltiq.API
```

---

## Camadas

### Domain

Núcleo da aplicação, **sem dependências externas**.

- **`BaseEntity`** — todas as entidades herdam desta classe. Fornece `Id` (GUID gerado automaticamente) e uma coleção de `DomainEvents`.
- **`AuditableEntity`** — estende `BaseEntity` com `CreatedAt`, `CreatedBy` e `UpdatedAt`.
- **`ValueObject`** — base para objetos de valor com igualdade por componentes.
- **`IDomainEvent` / `BaseDomainEvent`** — contrato e base para eventos de domínio (incluem `OccurredOn`).
- **`IRepository<T>`** — interface genérica de repositório (`GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `Update`, `Remove`).
- **`IUnitOfWork`** — interface para persistir mudanças (`SaveChangesAsync`).
- **`Result<T>`** — padrão railway-oriented para retornos de handlers (evita exceções em falhas esperadas).
- **Exceções** — `DomainException` (genérica, 400) e `NotFoundException` (recurso não encontrado, 404).

### Application

Contém todos os casos de uso como **Commands** e **Queries** via MediatR.

- Cada feature fica em `Features/<NomeDaFeature>/Commands/<NomeDoCommand>/` ou `.../Queries/`.
- Cada command/query tem seu próprio handler no mesmo diretório.
- Validators FluentValidation ficam no mesmo diretório do command/query e são executados automaticamente pelo `ValidationBehavior` antes do handler.
- Interfaces de infraestrutura (`IApplicationDbContext`, `ICurrentUserService`, `ITokenService`) são definidas aqui e implementadas na camada de Infrastructure.

### Infrastructure

Implementações de infraestrutura.

- **`ApplicationDbContext`** — EF Core com SQL Server. Aplica configurações da assembly automaticamente.
- **`Repository<T>`** — implementação genérica de `IRepository<T>` usando EF Core.
- **`UnitOfWork`** — delega `SaveChangesAsync` ao `ApplicationDbContext`.
- **`TokenService`** — gera tokens JWT com claims de `userId`, `userName` e `roles`.
- **`CurrentUserService`** — lê `userId`, `userName` e `isAuthenticated` do `HttpContext` via `IHttpContextAccessor`.

### API

Ponto de entrada da aplicação.

- **`BaseApiController`** — controller base com `[ApiController]` e `[Route("api/[controller]")]`. Injeta `IMediator`.
- **`ExceptionHandlers/`** — tratamento de exceções via `IExceptionHandler` (ver seção abaixo).
- **`Program.cs`** — registra todos os serviços e configura o pipeline.

---

## Convenções

### CQRS com MediatR

Toda lógica de caso de uso vive em `Features/` como commands ou queries:

```
Features/
└── Products/
    ├── Commands/
    │   └── CreateProduct/
    │       ├── CreateProductCommand.cs
    │       ├── CreateProductCommandHandler.cs
    │       └── CreateProductCommandValidator.cs
    └── Queries/
        └── GetProductById/
            ├── GetProductByIdQuery.cs
            └── GetProductByIdQueryHandler.cs
```

Nos controllers, use `Sender.Send(...)` para despachar:

```csharp
var result = await Sender.Send(new CreateProductCommand(...));
```

### Entidades de Domínio

```csharp
// Entidade simples
public class Product : BaseEntity
{
    public string Name { get; private set; }
}

// Entidade auditável
public class Order : AuditableEntity
{
    public Guid CustomerId { get; private set; }
}
```

- Sempre use GUID como chave primária.
- Levante eventos de domínio com `AddDomainEvent(new MeuEvento(...))`.

### Result Pattern

Handlers devem retornar `Result<T>` para falhas esperadas em vez de lançar exceções:

```csharp
// Sucesso
return Result.Success(produto);

// Falha esperada
return Result.Failure<Product>("Estoque insuficiente.");
```

Reserve exceções (`NotFoundException`, `DomainException`) para violações de invariantes ou recursos inexistentes.

### Validação

Crie um validator FluentValidation no mesmo diretório do command:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
```

O `ValidationBehavior` no pipeline MediatR executa todos os validators automaticamente e lança `ValidationException` (capturada pelo `ValidationExceptionHandler`) em caso de falha.

### Tratamento de Exceções

O tratamento de erros usa **`IExceptionHandler`** (ASP.NET Core) com um handler por tipo de exceção, registrados em cadeia no `Program.cs`. Todos os handlers retornam `application/problem+json`.

| Handler | Captura | HTTP Status |
|---|---|---|
| `ValidationExceptionHandler` | `FluentValidation.ValidationException` | `400 Bad Request` |
| `NotFoundExceptionHandler` | `NotFoundException` | `404 Not Found` |
| `UnauthorizedExceptionHandler` | `UnauthorizedAccessException` | `401 Unauthorized` |
| `GlobalExceptionHandler` | `Exception` (fallback) | `500 Internal Server Error` |

#### Formato das respostas

**400 — Validation Error**
```json
{
  "title": "Validation failed",
  "status": 400,
  "instance": "/api/products",
  "errors": [
    { "propertyName": "Name", "errorMessage": "'Name' must not be empty." },
    { "propertyName": "Price", "errorMessage": "'Price' must be greater than 0." }
  ]
}
```

**404 — Not Found**
```json
{
  "title": "Not Found",
  "status": 404,
  "instance": "/api/products/abc",
  "detail": "Entity 'Product' with key 'abc' was not found."
}
```

**401 — Unauthorized**
```json
{
  "title": "Unauthorized",
  "status": 401,
  "instance": "/api/orders"
}
```

**500 — Internal Server Error**
```json
{
  "title": "An unexpected error occurred.",
  "status": 500,
  "instance": "/api/checkout"
}
```

> **Campos restritos ao ambiente `Development`:**
> - `traceId` — incluído em todos os handlers
> - `stackTrace` — incluído apenas no `GlobalExceptionHandler`

#### Como lançar exceções nos handlers

```csharp
// Recurso não encontrado → 404
throw new NotFoundException(nameof(Product), id);

// Violação de regra de domínio → 400
throw new DomainException("O produto está inativo e não pode ser vendido.");
```

Nunca retorne status HTTP diretamente da camada Application.

### Repositório e Unit of Work

```csharp
// Leitura
var product = await _repository.GetByIdAsync(id, cancellationToken);
var products = await _repository.FindAsync(p => p.IsActive, cancellationToken);

// Escrita — sempre chame SaveChangesAsync via IUnitOfWork
await _repository.AddAsync(newProduct, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

Não chame `SaveChanges` diretamente em `IApplicationDbContext` nos handlers.

### Autenticação JWT

Autenticação via **Bearer Token**. O token é gerado por `ITokenService.GenerateToken(userId, userName, roles)` e validado automaticamente pelo middleware de autenticação JWT do ASP.NET Core.

Acesse o usuário autenticado via `ICurrentUserService`:

```csharp
var userId = _currentUserService.UserId;
var isAuthenticated = _currentUserService.IsAuthenticated;
```

---

## Testes

- **Framework:** xUnit
- **Mocks:** Moq
- **Assertions:** Shouldly
- **EF Core (Infrastructure):** InMemory provider

Os projetos de teste espelham a camada correspondente em `src/`. Coloque novos testes no projeto correto:

| Camada testada | Projeto de testes |
|---|---|
| `Voltiq.Domain` | `tests/Voltiq.Domain.Tests` |
| `Voltiq.Application` | `tests/Voltiq.Application.Tests` |
| `Voltiq.Infrastructure` | `tests/Voltiq.Infrastructure.Tests` |
