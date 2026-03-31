using System.Reflection;
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
            return CreateUnauthorized(error);
        }

        request.UserId = userId;
        return await next(cancellationToken);
    }

    private static TResponse CreateUnauthorized(Error error)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
        {
            var typeArg = responseType.GetGenericArguments()[0];
            var implicitOp = typeof(ErrorOr<>)
                .MakeGenericType(typeArg)
                .GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, [typeof(Error)])!;

            return (TResponse)implicitOp.Invoke(null, [error])!;
        }

        throw new InvalidOperationException($"Unexpected TResponse type: {responseType}");
    }
}
