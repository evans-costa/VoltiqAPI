using System.Reflection;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Voltiq.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures
            .Select(f => Error.Validation(code: f.PropertyName, description: f.ErrorMessage))
            .ToList();

        return CreateFailure(errors);
    }

    private static TResponse CreateFailure(List<Error> errors)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
        {
            var typeArg = responseType.GetGenericArguments()[0];
            var implicitOp = typeof(ErrorOr<>)
                .MakeGenericType(typeArg)
                .GetMethod("op_Implicit", BindingFlags.Static | BindingFlags.Public, [typeof(List<Error>)])!;

            return (TResponse)implicitOp.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException($"Unexpected TResponse type: {responseType}");
    }
}
