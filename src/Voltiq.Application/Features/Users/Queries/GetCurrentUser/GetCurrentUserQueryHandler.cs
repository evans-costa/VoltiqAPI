using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Users;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IRepository<User> userRepository)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<GetUserResponse>>
{
    public async Task<ErrorOr<GetUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
            return Error.NotFound(description: string.Format(
                ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), currentUserService.UserId));

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Error.NotFound(description: string.Format(
                ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), userId));

        return user.ToGetUserResponse();
    }
}
