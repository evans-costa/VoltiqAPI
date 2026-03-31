using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Materials.Commands.RegisterMaterial;

public sealed record RegisterMaterialRequest(
    string Name,
    decimal DefaultPrice,
    MaterialUnit Unit);
