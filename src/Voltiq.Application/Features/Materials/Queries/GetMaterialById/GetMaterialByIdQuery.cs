using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Materials.Queries.GetMaterialById;

public sealed record GetMaterialByIdQuery(Guid Id) : IAuthenticatedRequest<ErrorOr<MaterialResponse>>
{
    public Guid UserId { get; set; }
}
