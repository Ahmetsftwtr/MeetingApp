using System.Net;

namespace MeetingApp.Models.ReturnTypes.Concrete;

public class ErrorResult : Result
{
    public HttpStatusCode StatusCode { get; }

    public ErrorResult() : base(false)
    {
    }

    public ErrorResult(string message) : base(false, message)
    {
    }

    public ErrorResult(HttpStatusCode statusCode, string message) : base(false, message)
    {
        StatusCode = statusCode;
    }

    public ErrorResult(HttpStatusCode statusCode) : base(false)
    {
        StatusCode = statusCode;
    }
}