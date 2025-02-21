namespace ML.Application.Common.Exceptions;
public class HttpRequestException : Exception
{
    public HttpRequestException()
        : base("One or more HTTP request failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public HttpRequestException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public HttpRequestException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}