namespace Bookify.Application.Common;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Errors { get; set; }
    public PaginationInfo? Pagination { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Data = data, Success = true, Message = message ?? "Operation completed successfully." };

    public static ApiResponse<T> Ok(T data, PaginationInfo pagination, string? message = null)
        => new() { Data = data, Success = true, Message = message, Pagination = pagination };

    public static ApiResponse<T> Fail(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message ?? "Operation completed successfully." };

    public static ApiResponse Fail(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
