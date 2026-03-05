using MediatR;
using Voltiq.Domain.Entities;
using Voltiq.Domain.Interfaces;
using Voltiq.Exceptions.Exceptions;

namespace Voltiq.Application.Features.Users.Queries.GetUser;

public sealed class GetUserQueryHandler(IRepository<User> userRepository)
    : IRequestHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return new GetUserResponse(user.Name, user.Email.Value);
    }
}
