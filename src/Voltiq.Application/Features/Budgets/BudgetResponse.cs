using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Budgets;

public sealed record BudgetSummaryResponse(
    Guid Id,
    BudgetStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    BudgetClientSummaryResponse Client);

public sealed record BudgetClientSummaryResponse(Guid Id, string Name);

public sealed record BudgetDetailResponse(
    Guid Id,
    BudgetStatus Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    BudgetClientDetailResponse Client,
    IReadOnlyList<BudgetItemResponse> Items);

public sealed record BudgetClientDetailResponse(Guid Id, string Name, string Phone, string Email);

public sealed record BudgetItemResponse(
    Guid Id,
    Guid? MaterialId,
    string MaterialName,
    BudgetItemType Type,
    MaterialUnit? Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
