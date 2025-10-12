using System.Net;

namespace MeetingApp.Models.ReturnTypes.Concrete;

public class ErrorDataResult<T> : DataResult<T>
{
    public HttpStatusCode StatusCode { get; }

    public ErrorDataResult(T data) : base(data, false)
    {
    }

    public ErrorDataResult(T data, string message) : base(data, false, message)
    {
    }

    public ErrorDataResult(T data, string message, HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}