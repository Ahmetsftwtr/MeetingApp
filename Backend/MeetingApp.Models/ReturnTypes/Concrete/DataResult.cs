using MeetingApp.Models.ReturnTypes.Abstract;

namespace MeetingApp.Models.ReturnTypes.Concrete;

public class DataResult<T> : Result, IDataResult<T>
{
    public T Data { get; set; }

    public DataResult()
    {
        Data = default!;
    }

    public DataResult(T data, bool isSuccess, string message) : base(isSuccess, message)
    {
        Data = data;
    }

    public DataResult(T data, bool isSuccess) : base(isSuccess)
    {
        Data = data;
    }
}