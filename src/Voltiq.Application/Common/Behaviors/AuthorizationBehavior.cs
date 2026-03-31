using ErrorOr;
using MediatR;
using Voltiq.Application.Common.Interfaces;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuthenticatedRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId == Guid.Empty)
        {
            var error = Error.Unauthorized(description: ResourceErrorMessages.TITULO_NAO_AUTORIZADO);
            return (dynamic)error;
        }

        request.UserId = userId;
        return await next(cancellationToken);
    }
}
