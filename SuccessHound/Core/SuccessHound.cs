using SuccessHound.Abstractions;
using SuccessHound.Config;

namespace SuccessHound.Core;

/// <summary>
/// Main entry point for SuccessHound configuration and response wrapping
/// </summary>
public static class SuccessHound
{
    private static ISuccessResponseFactory? _responseFactory;
    private static object? _paginationFactory;

    /// <summary>
    /// Configure SuccessHound and its optional extensions
    /// </summary>
    /// <param name="configure">Configuration action</param>
    /// <exception cref="InvalidOperationException">Thrown if response factory is not configured</exception>
    public static void Configure(Action<SuccessHoundConfiguration> configure)
    {
        var config = new SuccessHoundConfiguration();
        configure(config);

        // Response factory is required
        _responseFactory = config.ResponseFactory
            ?? throw new InvalidOperationException(
                "Response factory not configured. Call config.UseDefaultApiResponse() or config.UseApiResponse() in SuccessHound.Configure()");

        // Pagination factory is optional
        _paginationFactory = config.PaginationFactory;
    }

    /// <summary>
    /// Wrap raw data using the configured factory
    /// </summary>
    /// <param name="data">The raw data to wrap</param>
    /// <returns>Wrapped response object</returns>
    /// <exception cref="InvalidOperationException">Thrown if SuccessHound is not configured</exception>
    public static object Wrap(object? data)
    {
        var factory = GetResponseFactory();
        return factory.Wrap(data);
    }

    /// <summary>
    /// Wrap raw data with metadata using the configured factory
    /// </summary>
    /// <param name="data">The raw data to wrap</param>
    /// <param name="meta">Optional metadata to include</param>
    /// <returns>Wrapped response object</returns>
    /// <exception cref="InvalidOperationException">Thrown if SuccessHound is not configured</exception>
    public static object Wrap(object? data, object? meta)
    {
        var factory = GetResponseFactory();
        return factory.Wrap(data, meta);
    }

    /// <summary>
    /// Get the configured response factory
    /// </summary>
    /// <returns>The configured response factory</returns>
    /// <exception cref="InvalidOperationException">Thrown if SuccessHound is not configured</exception>
    internal static ISuccessResponseFactory GetResponseFactory()
    {
        return _responseFactory
            ?? throw new InvalidOperationException(
                "SuccessHound is not configured. Call SuccessHound.Configure() in your Program.cs");
    }

    /// <summary>
    /// Get the configured pagination factory (if pagination is enabled)
    /// </summary>
    /// <returns>The configured pagination factory</returns>
    /// <exception cref="InvalidOperationException">Thrown if pagination is not configured</exception>
    public static dynamic GetPaginationFactory()
    {
        return _paginationFactory
            ?? throw new InvalidOperationException(
                "Pagination is not configured. Call config.UsePagination() in SuccessHound.Configure(). " +
                "Make sure you have the SuccessHound.Pagination package installed.");
    }

    /// <summary>
    /// Check if pagination is enabled
    /// </summary>
    public static bool IsPaginationEnabled => _paginationFactory != null;
}
