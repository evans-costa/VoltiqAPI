using ErrorOr;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Features.Users.Queries.GetCurrentUser;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IAuthenticatedRequest<ErrorOr<GetUserResponse>>
{
    public Guid UserId { get; set; }
}
