using ErrorOr;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Materials.Commands.RegisterMaterial;

public sealed record RegisterMaterialCommand(
    string Name,
    decimal DefaultPrice,
    MaterialUnit Unit) : IAuthenticatedRequest<ErrorOr<MaterialResponse>>
{
    public Guid UserId { get; set; }
}
