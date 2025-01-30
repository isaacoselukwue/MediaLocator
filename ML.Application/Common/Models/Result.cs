namespace ML.Application.Common.Models;
public class Result
{
    internal Result(bool succeeded, string message, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Message = message;
        Errors = errors.ToArray();
    }
    public bool Succeeded { get; init; }
    public string Message { get; init; }
    public string[] Errors { get; init; }
    public static Result Success(string message) => new(true, message, []);
    public static Result Failure(string message, IEnumerable<string> errors) => new(false, message, errors);
}

public class Result<T> : Result
{
    internal Result(bool succeeded, string message, IEnumerable<string> errors, T data)
        : base(succeeded, message, errors)
    {
        Data = data;
    }

    public T? Data { get; init; }

    public static new Result Success(string message) => new(true, message, Array.Empty<string>());
    public static Result<T> Success(string message, T data) => new(true, message, Array.Empty<string>(), data);
    public static new Result Failure(string message, IEnumerable<string> errors) => new(false, message, errors);
}
