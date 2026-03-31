using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Materials;

public sealed record MaterialResponse(
    Guid Id,
    string Name,
    decimal DefaultPrice,
    MaterialUnit Unit,
    bool IsActive);
