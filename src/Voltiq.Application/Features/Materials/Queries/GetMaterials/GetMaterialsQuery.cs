using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Materials.Queries.GetMaterials;

public sealed record GetMaterialsQuery : IAuthenticatedRequest<ErrorOr<IReadOnlyList<MaterialResponse>>>
{
    public Guid UserId { get; set; }
}
