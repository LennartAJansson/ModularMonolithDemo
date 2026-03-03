namespace WorkloadsModule.Core;

public class Result(ResultStatus status, string? error = null)
{
    public ResultStatus Status { get; init; } = status;
    public string? Error { get; init; } = error;
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success);
    public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
    public static Result Invalid(string error) => new(ResultStatus.Invalid, error);
    public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
    public static Result Failure(string error) => new(ResultStatus.Failure, error);
}

public sealed class Result<T>(ResultStatus status, T? value = default, string? error = null) : Result(status, error)
{
    public T? Value { get; init; } = value;

    public static Result<T> Success(T value) => new(ResultStatus.Success, value);
    public new static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);
    public new static Result<T> Invalid(string error) => new(ResultStatus.Invalid, default, error);
    public new static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error);
    public new static Result<T> Failure(string error) => new(ResultStatus.Failure, default, error);
}
