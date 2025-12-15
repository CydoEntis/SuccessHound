namespace SuccessHound.Abstractions;

/// <summary>
/// Factory interface for creating custom success response wrappers
/// </summary>
public interface ISuccessResponseFactory
{
    /// <summary>
    /// Wraps successful data in the user-defined response shape.
    /// </summary>
    /// <param name="data">The raw data returned by the handler</param>
    /// <param name="meta">Optional metadata to include in the response</param>
    object Wrap(object? data, object? meta = null);
}