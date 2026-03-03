using System.Diagnostics.CodeAnalysis;

namespace Voltiq.Domain.Common;

public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}

public class Result<T> : Result
{
    private Result(T value) : base(true, null) => Value = value;
    private Result(string error) : base(false, error) { }

    [field: AllowNull, MaybeNull]
    public T Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("Cannot access Value of a failed Result.");

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error) => new(error);
}
