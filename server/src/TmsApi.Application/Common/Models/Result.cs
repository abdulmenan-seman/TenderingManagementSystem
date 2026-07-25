namespace TmsApi.Application.Common.Models;

public class Result<TValue>
{
    public bool IsSuccess { get; }
    public TValue? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, TValue? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TValue> Success(TValue value) => new(true, value, null);
    public static Result<TValue> Failure(string error) => new(false, default, error);
}