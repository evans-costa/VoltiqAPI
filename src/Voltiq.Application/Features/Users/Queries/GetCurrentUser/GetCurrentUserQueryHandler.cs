using ErrorOr;
using MediatR;
using Voltiq.Application.Mappings.Users;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories.User;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IUserReadOnlyRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, ErrorOr<GetUserResponse>>
{
    public async Task<ErrorOr<GetUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound(description: string.Format(
                ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), request.UserId));

        return user.ToGetUserResponse();
    }
}
