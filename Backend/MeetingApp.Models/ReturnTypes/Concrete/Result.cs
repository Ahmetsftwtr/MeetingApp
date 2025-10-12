using MeetingApp.Models.ReturnTypes.Abstract;

namespace MeetingApp.Models.ReturnTypes.Concrete;

public class Result : IResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    public Result()
    {
        Message = string.Empty;
    }

    public Result(bool isSuccess, string message) : this(isSuccess)
    {
        Message = message;
    }

    public Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
        Message = string.Empty;
    }
}