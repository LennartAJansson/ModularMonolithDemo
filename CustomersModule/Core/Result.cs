namespace CustomersModule.Core;

public record Result(ResultStatus Status, string? Error = null)
{
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success);
    public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
    public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
    public static Result Invalid(string error) => new(ResultStatus.Invalid, error);
    public static Result Failure(string error) => new(ResultStatus.Failure, error);
}

public sealed record Result<T>(ResultStatus Status, T? Value, string? Error = null)
    : Result(Status, Error)
{
    public static Result<T> Success(T value) => new(ResultStatus.Success, value);
    public new static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);
    public new static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error);
    public new static Result<T> Invalid(string error) => new(ResultStatus.Invalid, default, error);
    public new static Result<T> Failure(string error) => new(ResultStatus.Failure, default, error);

    public static Result<T> From(Result other) => new(other.Status, default, other.Error);
}
