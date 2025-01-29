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
