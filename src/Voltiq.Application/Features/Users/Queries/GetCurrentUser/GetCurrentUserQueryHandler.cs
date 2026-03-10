using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Application.Mappings.Users;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IRepository<User> userRepository)
    : IRequestHandler<GetCurrentUserQuery, Result<GetUserResponse>>
{
    public async Task<Result<GetUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserService.UserId, out var userId) || userId == Guid.Empty)
            return Result<GetUserResponse>.Failure(
                new NotFoundError(string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), currentUserService.UserId)));

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
            return Result<GetUserResponse>.Failure(
                new NotFoundError(string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), userId)));

        return Result<GetUserResponse>.Success(user.ToGetUserResponse());
    }
}
