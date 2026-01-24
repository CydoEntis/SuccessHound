namespace SuccessHound.Defaults;

/// <summary>
/// Generic API response envelope with strongly-typed metadata.
/// </summary>
/// <typeparam name="TData">The type of the response data</typeparam>
/// <typeparam name="TMeta">The type of the metadata</typeparam>
public class ApiResponse<TData, TMeta>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The response payload
    /// </summary>
    public TData? Data { get; init; }

    /// <summary>
    /// Optional metadata (pagination, versioning, etc.)
    /// </summary>
    public TMeta? Meta { get; init; }

    /// <summary>
    /// UTC timestamp when the response was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    protected ApiResponse()
    {
    }

    /// <summary>
    /// Creates a successful response wrapping the given data and metadata.
    /// </summary>
    public static ApiResponse<TData, TMeta> Ok(TData data, TMeta meta) =>
        new() { Success = true, Data = data, Meta = meta };
}

/// <summary>
/// Marker type representing absence of metadata.
/// </summary>
public sealed class NoMeta
{
    internal static readonly NoMeta Instance = new();
    private NoMeta() { }
}

/// <summary>
/// Generic API response envelope without metadata (backward-compatible wrapper).
/// </summary>
/// <typeparam name="T">The type of the response data</typeparam>
public class ApiResponse<T> : ApiResponse<T, NoMeta>
{
    private ApiResponse()
    {
    }

    /// <summary>
    /// Creates a successful response wrapping the given data.
    /// </summary>
    public static ApiResponse<T> Ok(T data) =>
        new()
        {
            Success = true,
            Data = data,
            Meta = NoMeta.Instance
        };
}