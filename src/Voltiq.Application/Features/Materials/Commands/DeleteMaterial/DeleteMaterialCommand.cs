using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Materials.Commands.DeleteMaterial;

public sealed record DeleteMaterialCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Deleted>>
{
    public Guid UserId { get; set; }
}
