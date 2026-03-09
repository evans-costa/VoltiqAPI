using MediatR;
using Voltiq.Domain.Common;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces.Repositories;
using Voltiq.Exceptions.Errors;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Features.Users.Queries.GetUser;

public sealed class GetUserQueryHandler(IRepository<User> userRepository)
    : IRequestHandler<GetUserQuery, Result<GetUserResponse>>
{
    public async Task<Result<GetUserResponse>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            return Result<GetUserResponse>.Failure(
                new NotFoundError(string.Format(ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA, nameof(User), request.Id)));

        return Result<GetUserResponse>.Success(new GetUserResponse(user.Name, user.Email.Value));
    }
}
