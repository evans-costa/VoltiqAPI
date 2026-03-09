using FluentValidation;
using MediatR;
using Voltiq.Domain.Common;
using Voltiq.Exceptions.Errors;

namespace Voltiq.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
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
            .Select(Error (f) => new ValidationError(f.PropertyName, f.ErrorMessage))
            .ToList();

        return CreateFailure(errors);
    }

    private static TResponse CreateFailure(List<Error> errors)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
            return (TResponse)Result.Failure(errors);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var typeArg = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(typeArg)
                .GetMethod(nameof(Result<object>.Failure), [typeof(IReadOnlyList<Error>)])!;

            return (TResponse)failureMethod.Invoke(null, [errors.AsReadOnly()])!;
        }

        throw new InvalidOperationException($"Unexpected TResponse type: {responseType}");
    }
}
