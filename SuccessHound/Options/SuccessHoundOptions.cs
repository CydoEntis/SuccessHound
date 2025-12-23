using SuccessHound.Abstractions;

namespace SuccessHound.Options;

/// <summary>
/// Configuration options for SuccessHound
/// </summary>
public sealed class SuccessHoundOptions
{
    internal Type? FormatterType { get; private set; }

    /// <summary>
    /// Gets or sets the pagination factory instance (for use by SuccessHound.Pagination package)
    /// </summary>
    public object? PaginationFactory { get; set; }

    /// <summary>
    /// Configure SuccessHound to use a specific formatter type
    /// </summary>
    /// <typeparam name="T">The formatter type that implements ISuccessResponseFormatter</typeparam>
    public void UseFormatter<T>()
        where T : class, ISuccessResponseFormatter
    {
        FormatterType = typeof(T);
    }
}
