namespace SuccessHound.Defaults;

/// <summary>
/// Generic API response envelope for SuccessHound.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public object? Meta { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    private ApiResponse()
    {
    }

    /// <summary>
    /// Creates a successful response wrapping the given data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, object? meta = null) =>
        new() { Success = true, Data = data, Meta = meta };
}