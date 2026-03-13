namespace EduPortal.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }

    private Result() { }

    public static Result<T> Success(T value) =>
        new() { IsSuccess = true, Value = value, StatusCode = 200 };

    public static Result<T> Created(T value) =>
        new() { IsSuccess = true, Value = value, StatusCode = 201 };

    public static Result<T> Failure(string error, int statusCode = 400) =>
        new() { IsSuccess = false, Error = error, StatusCode = statusCode };

    public static Result<T> NotFound(string message = "Resource not found.") =>
        new() { IsSuccess = false, Error = message, StatusCode = 404 };

    public static Result<T> Unauthorized(string message = "Unauthorized.") =>
        new() { IsSuccess = false, Error = message, StatusCode = 401 };

    public static Result<T> Conflict(string message) =>
        new() { IsSuccess = false, Error = message, StatusCode = 409 };
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }

    private Result() { }

    public static Result Success() => new() { IsSuccess = true, StatusCode = 200 };
    public static Result Failure(string error, int statusCode = 400) => new() { IsSuccess = false, Error = error, StatusCode = statusCode };
    public static Result NotFound(string message = "Not found.") => new() { IsSuccess = false, Error = message, StatusCode = 404 };
}
