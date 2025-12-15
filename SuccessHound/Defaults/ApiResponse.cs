namespace SuccessHound.Defaults;

/// <summary>
/// Generic API response envelope for SuccessHound.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The response payload
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Optional metadata (pagination, versioning, etc.)
    /// </summary>
    public object? Meta { get; init; }

    /// <summary>
    /// UTC timestamp when the response was created
    /// </summary>
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