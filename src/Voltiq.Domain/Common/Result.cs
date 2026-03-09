using System.Diagnostics.CodeAnalysis;
using Voltiq.Exceptions.Errors;

namespace Voltiq.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, IReadOnlyList<Error>? errors)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? [];
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }
    public Error? FirstError => Errors.Count > 0 ? Errors[0] : null;

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure(IReadOnlyList<Error> errors) => new(false, errors);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
    public static Result<T> Failure<T>(IReadOnlyList<Error> errors) => Result<T>.Failure(errors);
}

public class Result<T> : Result
{
    private Result(T value) : base(true, null) => Value = value;
    private Result(IReadOnlyList<Error> errors) : base(false, errors) { }

    [field: AllowNull, MaybeNull]
    public T Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new([error]);
    public new static Result<T> Failure(IReadOnlyList<Error> errors) => new(errors);
}
