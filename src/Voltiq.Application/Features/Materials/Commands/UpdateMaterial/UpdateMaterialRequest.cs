using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Materials.Commands.UpdateMaterial;

public sealed record UpdateMaterialRequest(
    string Name,
    decimal DefaultPrice,
    MaterialUnit Unit);
