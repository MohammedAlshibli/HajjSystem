namespace HajjSystem.Application.Common.Models;

/// <summary>
/// Discriminated union: either Success or Failure with a message.
/// Services return Result instead of throwing exceptions.
/// </summary>
public class Result
{
    public bool    Succeeded { get; }
    public string  Error     { get; } = string.Empty;

    protected Result(bool ok, string error = "")
    {
        Succeeded = ok;
        Error     = error;
    }

    public static Result Success()             => new(true);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Fail(error);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool ok, T? value, string error = "") : base(ok, error)
    {
        Value = value;
    }

    public static Result<T> Ok(T value)         => new(true, value);
    public static Result<T> Fail(string error)  => new(false, default, error);
}
