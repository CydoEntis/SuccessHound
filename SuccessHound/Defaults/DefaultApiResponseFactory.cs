using SuccessHound.Abstractions;

namespace SuccessHound.Defaults;

/// <summary>
/// Default formatter that wraps any data type into the <see cref="ApiResponse{T}"/> or <see cref="ApiResponse{TData, TMeta}"/> envelope.
/// Can be used out-of-the-box with SuccessHound for minimal setup.
/// Supports both strongly-typed metadata and backward-compatible object metadata.
/// </summary>
public sealed class DefaultSuccessFormatter : ISuccessResponseFormatter
{
    /// <summary>
    /// Formats the given data into <see cref="ApiResponse{T}"/> or <see cref="ApiResponse{TData, TMeta}"/> dynamically.
    /// Handles null data and optional metadata.
    /// When metadata is provided, attempts to use strongly-typed ApiResponse{TData, TMeta} if possible.
    /// </summary>
    /// <param name="data">The raw data to format.</param>
    /// <param name="meta">Optional metadata to include in the response.</param>
    /// <returns>An ApiResponse{T} or ApiResponse{TData, TMeta} object containing the data.</returns>
    public object Format(object? data, object? meta = null)
    {
        // If no metadata, use the simple ApiResponse<T> wrapper
        if (meta == null)
        {
            var type = typeof(ApiResponse<>).MakeGenericType(data?.GetType() ?? typeof(object));
            var okMethod = type.GetMethod("Ok", new[] { data?.GetType() ?? typeof(object) })!;
            return okMethod.Invoke(null, new object?[] { data })!;
        }

        // If metadata is provided, use ApiResponse<TData, TMeta>
        var dataType = data?.GetType() ?? typeof(object);
        var metaType = meta.GetType();
        var genericType = typeof(ApiResponse<,>).MakeGenericType(dataType, metaType);
        var genericOkMethod = genericType.GetMethod("Ok", new[] { dataType, metaType })!;
        return genericOkMethod.Invoke(null, new object?[] { data, meta })!;
    }
}