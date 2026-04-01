using ErrorOr;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Domain.Enums;

namespace Voltiq.Application.Features.Materials.Commands.UpdateMaterial;

public sealed record UpdateMaterialCommand(
    Guid Id,
    string Name,
    decimal DefaultPrice,
    MaterialUnit Unit) : IAuthenticatedRequest<ErrorOr<Updated>>
{
    public Guid UserId { get; set; }
}
