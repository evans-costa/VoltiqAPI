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
- [Endpoints](#endpoints)
  - [Usuários](#usuários)
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
Voltiq.Exceptions  → (sem dependências Voltiq)
Voltiq.API         → Voltiq.Application + Voltiq.Infrastructure + Voltiq.Exceptions
Voltiq.Application → Voltiq.Domain  (Voltiq.Exceptions transitivo)
Voltiq.Infrastructure → Voltiq.Application + Voltiq.Domain
Voltiq.Domain      → Voltiq.Exceptions
```

---

## Estrutura de Pastas

```
voltiq/
├── src/
│   ├── Voltiq.API/                        # Entrada da aplicação (controllers, handlers de exceção, DI)
│   │   ├── Controllers/
│   │   │   └── BaseApiController.cs       # Controller base com MediatR + ToErrorResult → ProblemDetails
│   │   ├── ExceptionHandlers/             # IExceptionHandler para erros inesperados
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
│   ├── Voltiq.Domain/                     # Núcleo do domínio
│   │   ├── Common/
│   │   │   └── Result.cs                  # Result pattern (railway-oriented)
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs              # Id (GUID) + DomainEvents
│   │   │   └── AuditableEntity.cs         # + CreatedAt, CreatedBy, UpdatedAt
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs
│   │   │   └── BaseDomainEvent.cs
│   │   ├── Interfaces/
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── Repositories/
│   │   │       ├── IRepository.cs
│   │   │       └── User/
│   │   │           └── IUserRepository.cs
│   │   └── ValueObjects/
│   │       └── ValueObject.cs
│   │
│   ├── Voltiq.Exceptions/                 # Tipos de erro + exceções + mensagens centralizadas
│   │   ├── Errors/
│   │   │   ├── Error.cs                   # Classe base de erro tipado
│   │   │   ├── ValidationError.cs         # Erro de validação (com PropertyName)
│   │   │   ├── NotFoundError.cs           # Recurso não encontrado
│   │   │   └── ConflictError.cs           # Conflito de unicidade
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs         # Exceção de invariante de domínio
│   │   │   └── NotFoundException.cs       # Recurso não encontrado (exception)
│   │   └── Resources/
│   │       ├── ResourceErrorMessages.resx # Todas as mensagens de erro (pt-BR)
│   │       └── ResourceErrorMessages.Designer.cs
│   │
│   └── Voltiq.Infrastructure/             # EF Core, JWT, repositórios
│       ├── Auth/
│       │   ├── TokenService.cs
│       │   └── CurrentUserService.cs
│       ├── Persistence/
│       │   ├── ApplicationDbContext.cs
│       │   └── Repositories/
│       │       ├── Repository.cs           # Repositório genérico
│       │       ├── UnitOfWork.cs
│       │       └── User/
│       │           └── UserRepository.cs   # Repositório especializado (ExistsUserAsync)
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
- PostgreSQL (local ou via Docker)

---

## Configuração

Edite `src/Voltiq.API/appsettings.json` (ou use variáveis de ambiente / User Secrets em desenvolvimento):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=voltiq;Username=postgres;Password=SUA_SENHA"
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
| `ConnectionStrings:DefaultConnection` | Connection string do PostgreSQL (formato Npgsql) |
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
- **`IRepository<T>`** — interface genérica de repositório (`GetByIdAsync`, `AddAsync`, `Update`, `Remove`).
- **`IUnitOfWork`** — interface para persistir mudanças (`SaveChangesAsync`).
- **`Result` / `Result<T>`** — padrão railway-oriented com erros tipados (`Error`, `ValidationError`, `NotFoundError`, `ConflictError`). Handlers retornam `Result<T>` para falhas esperadas em vez de lançar exceções.
- **Exceções** — `DomainException` (violação de invariante de domínio) e `NotFoundException` (recurso não encontrado) — reservadas para falhas fora do pipeline MediatR.

### Application

Contém todos os casos de uso como **Commands** e **Queries** via MediatR.

- Cada feature fica em `Features/<NomeDaFeature>/Commands/<NomeDoCommand>/` ou `.../Queries/`.
- Cada command/query tem seu próprio handler no mesmo diretório.
- Validators FluentValidation ficam no mesmo diretório do command/query e são executados automaticamente pelo `ValidationBehavior` antes do handler.
- Interfaces de infraestrutura (`IApplicationDbContext`, `ICurrentUserService`, `ITokenService`) são definidas aqui e implementadas na camada de Infrastructure.

### Infrastructure

Implementações de infraestrutura.

- **`ApplicationDbContext`** — EF Core com PostgreSQL (Npgsql). Aplica configurações da assembly automaticamente.
- **`Argon2PasswordHasher`** — implementa `IPasswordHasher` usando **Argon2id** (via `Konscious.Security.Cryptography.Argon2`). Parâmetros: 4 iterações, 64 MB de memória, paralelismo 2.
- **`Repository<T>`** — implementação genérica de `IRepository<T>` usando EF Core.
- **`UnitOfWork`** — delega `SaveChangesAsync` ao `ApplicationDbContext`.
- **`TokenService`** — gera tokens JWT com claims de `userId`, `userName` e `roles`.
- **`CurrentUserService`** — lê `userId`, `userName` e `isAuthenticated` do `HttpContext` via `IHttpContextAccessor`.

### API

Ponto de entrada da aplicação.

- **`BaseApiController`** — controller base com `[ApiController]` e `[Route("api/[controller]")]`. Injeta `ISender` via `HttpContext.RequestServices`. Expõe `ToErrorResult(Result)` que converte erros tipados do `Result` em `ProblemDetails` (`ValidationProblemDetails` para 400, `ProblemDetails` para 404/409/500).
- **`ExceptionHandlers/`** — tratamento de exceções não esperadas via `IExceptionHandler` (`UnauthorizedExceptionHandler` → 401, `GlobalExceptionHandler` → 500).
- **`Program.cs`** — registra todos os serviços e configura o pipeline.

---

## Endpoints

### Usuários

#### `POST /api/users` — Criar usuário

Cria um novo usuário na plataforma.

**Request body:**
```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "document": "529.982.247-25",
  "password": "MinhaS3nh@Segura"
}
```

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `name` | string | ✅ | Nome completo do usuário |
| `email` | string | ✅ | Endereço de e-mail válido e único |
| `document` | string | ✅ | CPF (11 dígitos) ou CNPJ (14 dígitos), com ou sem pontuação |
| `password` | string | ✅ | Mínimo 8 caracteres; armazenada como hash Argon2id |

**Respostas:**

| Status | Descrição |
|---|---|
| `201 Created` | `{ "id": "<guid>" }` |
| `400 Bad Request` | Erro de validação (campos inválidos) |
| `409 Conflict` | E-mail ou CPF/CNPJ já cadastrado |

**Regras de validação:**
- E-mail deve ser único no sistema.
- CPF e CNPJ são validados com verificação de dígitos verificadores. CPF com todos os dígitos iguais (e.g., `111.111.111-11`) são rejeitados.
- Documento deve ser único no sistema.
- Senha é armazenada como hash **Argon2id** — nunca em texto puro.

---



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

Handlers retornam `Result<T>` (ou `Result` para operações sem retorno como Update/Delete) com **erros tipados**:

```csharp
// Sucesso
return Result<Guid>.Success(user.Id);

// Sucesso sem valor (para Update/Delete → 204 No Content)
return Result.Success();

// Falha esperada — conflito (409)
return Result<Guid>.Failure(new ConflictError("Já existe um usuário com este e-mail."));

// Falha esperada — não encontrado (404)
return Result<GetUserResponse>.Failure(
    new NotFoundError(string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), id)));
```

**Hierarquia de erros** (em `Voltiq.Exceptions/Errors/`):

| Tipo | Uso | HTTP Status |
|---|---|---|
| `Error` (base) | Erro genérico | 500 |
| `ValidationError` | Falha de validação (com `PropertyName`) | 400 |
| `NotFoundError` | Recurso não encontrado | 404 |
| `ConflictError` | Conflito de unicidade | 409 |

Reserve exceções (`DomainException`) para violações de invariantes de domínio na camada Domain.

### Validação

Crie um validator FluentValidation no mesmo diretório do command:

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);

        // Validação de formato de Value Objects via TryParse
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ResourceErrorMessages.USUARIO_EMAIL_OBRIGATORIO)
            .Must(email => Email.TryParse(email, out _, out _))
            .WithMessage(ResourceErrorMessages.USUARIO_EMAIL_INVALIDO);
    }
}
```

O `ValidationBehavior` no pipeline MediatR:
1. Executa todos os validators automaticamente **antes** do handler.
2. Se houver falhas, retorna `Result.Failure(...)` com `ValidationError` tipados — **sem lançar exceções**.
3. O controller recebe o `Result` com `IsFailure == true` e converte para `ValidationProblemDetails` (400).

> **Constraint:** o `ValidationBehavior` exige `where TResponse : Result`, portanto todo command/query deve retornar `Result` ou `Result<T>`.

### Tratamento de Erros

O tratamento de erros usa uma abordagem **dual**:

**1. Erros esperados → `Result` com erros tipados (dentro do pipeline MediatR)**

O handler retorna `Result.Failure(new ConflictError(...))` ou o `ValidationBehavior` retorna `Result.Failure(new ValidationError(...))`. O controller converte via `ToErrorResult(result)` em `ProblemDetails` nativo do ASP.NET Core:

| Tipo de Erro | HTTP Status | Resposta |
|---|---|---|
| `ValidationError` | `400 Bad Request` | `ValidationProblemDetails` com `errors` dict |
| `NotFoundError` | `404 Not Found` | `ProblemDetails` com `detail` |
| `ConflictError` | `409 Conflict` | `ProblemDetails` com `detail` |

**2. Erros inesperados → `IExceptionHandler` (fora do pipeline MediatR)**

Para exceções não capturadas e exceções de autenticação, `IExceptionHandler` converte em `application/problem+json`:

| Handler | Captura | HTTP Status |
|---|---|---|
| `UnauthorizedExceptionHandler` | `UnauthorizedAccessException` | `401 Unauthorized` |
| `GlobalExceptionHandler` | `Exception` (fallback) | `500 Internal Server Error` |

### Mensagens de Erro Centralizadas

Todas as mensagens de erro estão centralizadas em **`src/Voltiq.Exceptions/Resources/ResourceErrorMessages.resx`**, todas em **pt-BR**.

- Acesse via classe fortemente tipada `ResourceErrorMessages` do namespace `Voltiq.Exceptions.Resources`.
- **Nunca** hardcode strings de erro em arquivos-fonte — adicione uma nova chave ao `.resx` primeiro.
- Chaves com parâmetros usam `{0}`, `{1}`, etc.; use `string.Format(ResourceErrorMessages.Chave, ...)` no ponto de uso.
- Grupos de chaves: erros de domínio, erros de aplicação (por feature), títulos HTTP da API.

#### Formato das respostas

**400 — Validation Error** (`ValidationProblemDetails`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Falha de validação",
  "status": 400,
  "instance": "/api/users",
  "errors": {
    "Email": ["O e-mail informado não é válido."],
    "Password": ["A senha deve ter pelo menos 8 caracteres."]
  }
}
```

**404 — Not Found** (`ProblemDetails`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Não encontrado",
  "status": 404,
  "instance": "/api/users/abc",
  "detail": "A entidade 'User' com a chave 'abc' não foi encontrada."
}
```

**409 — Conflict** (`ProblemDetails`)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflito",
  "status": 409,
  "instance": "/api/users",
  "detail": "Já existe um usuário cadastrado com este e-mail."
}
```

**401 — Unauthorized**
```json
{
  "title": "Não autorizado",
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

#### Como retornar erros nos handlers

```csharp
// Recurso não encontrado → 404
return Result<GetUserResponse>.Failure(
    new NotFoundError(string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), id)));

// Conflito de unicidade → 409
return Result<Guid>.Failure(
    new ConflictError(ResourceErrorMessages.USUARIO_EMAIL_JA_CADASTRADO));
```

No controller:
```csharp
var result = await Sender.Send(new GetUserQuery(id), cancellationToken);

if (result.IsFailure)
    return ToErrorResult(result);  // Converte para ProblemDetails automaticamente

return Ok(result.Value);
```

Nunca retorne status HTTP diretamente da camada Application.

### Repositório e Unit of Work

```csharp
// Leitura
var product = await _repository.GetByIdAsync(id, cancellationToken);

// Escrita — sempre chame SaveChangesAsync via IUnitOfWork
await _repository.AddAsync(newProduct, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);

// Queries especializadas — defina no repositório específico (ex: IUserRepository)
var exists = await _userRepository.ExistsUserAsync(document, email, cancellationToken);
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
- **Testes de integração (Infrastructure):** [Testcontainers](https://dotnet.testcontainers.org/) com `Testcontainers.PostgreSql` — sobe um container real de PostgreSQL para cada execução de teste.

### Workflow TDD

Todo o projeto segue o padrão **red → green → refactor**:

1. **Red** — escreva o teste antes da implementação
2. **Green** — implemente o mínimo para o teste passar
3. **Refactor** — melhore o código sem quebrar os testes

A primeira commit em uma branch de feature deve conter apenas os testes (`test: ...`). Os commits de implementação vêm em seguida.

Os projetos de teste espelham a camada correspondente em `src/`. Coloque novos testes no projeto correto:

| Camada testada | Projeto de testes |
|---|---|
| `Voltiq.Domain` | `tests/Voltiq.Domain.Tests` |
| `Voltiq.Application` | `tests/Voltiq.Application.Tests` |
| `Voltiq.Infrastructure` | `tests/Voltiq.Infrastructure.Tests` |
