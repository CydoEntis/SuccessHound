namespace SuccessHound.Abstractions;

/// <summary>
/// Formatter interface for creating custom success response wrappers
/// </summary>
public interface ISuccessResponseFormatter
{
    /// <summary>
    /// Formats successful data in the user-defined response shape.
    /// </summary>
    /// <param name="data">The raw data returned by the handler</param>
    /// <param name="meta">Optional metadata to include in the response</param>
    object Format(object? data, object? meta = null);
}