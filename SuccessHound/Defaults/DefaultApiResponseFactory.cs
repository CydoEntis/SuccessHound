using SuccessHound.Abstractions;

namespace SuccessHound.Defaults;

/// <summary>
/// Default factory that wraps any data type into the <see cref="ApiResponse{T}"/> envelope.
/// Can be used out-of-the-box with SuccessHound for minimal setup.
/// </summary>
public class DefaultApiResponseFactory : ISuccessResponseFactory
{
    /// <summary>
    /// Wraps the given data into <see cref="ApiResponse{T}"/> dynamically.
    /// Handles null data and optional metadata.
    /// </summary>
    /// <param name="data">The raw data to wrap.</param>
    /// <param name="meta">Optional metadata to include in the response.</param>
    /// <returns>An ApiResponse{T} object containing the data.</returns>
    public object Wrap(object? data, object? meta = null)
    {
        var type = typeof(ApiResponse<>).MakeGenericType(data?.GetType() ?? typeof(object));
        var okMethod = type.GetMethod("Ok")!;

        return okMethod.Invoke(null, new object?[] { data, meta })!;
    }
}