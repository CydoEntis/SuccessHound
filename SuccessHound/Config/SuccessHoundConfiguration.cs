using SuccessHound.Abstractions;

namespace SuccessHound.Config;

/// <summary>
/// Configuration options for SuccessHound and its extensions
/// </summary>
public class SuccessHoundConfiguration
{
    internal ISuccessResponseFactory? ResponseFactory { get; private set; }
    internal object? PaginationFactory { get; private set; }

    /// <summary>
    /// Use the default API response factory (ApiResponse&lt;T&gt;)
    /// </summary>
    /// <returns>Configuration instance for method chaining</returns>
    public SuccessHoundConfiguration UseDefaultApiResponse()
    {
        ResponseFactory = new Defaults.DefaultApiResponseFactory();
        return this;
    }

    /// <summary>
    /// Use a custom API response factory
    /// </summary>
    /// <param name="factory">Your custom factory implementation</param>
    /// <returns>Configuration instance for method chaining</returns>
    public SuccessHoundConfiguration UseApiResponse(ISuccessResponseFactory factory)
    {
        ResponseFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    /// <summary>
    /// Use a custom API response factory by type
    /// </summary>
    /// <typeparam name="TFactory">Factory type with parameterless constructor</typeparam>
    /// <returns>Configuration instance for method chaining</returns>
    public SuccessHoundConfiguration UseApiResponse<TFactory>()
        where TFactory : ISuccessResponseFactory, new()
    {
        ResponseFactory = new TFactory();
        return this;
    }

    /// <summary>
    /// Sets pagination factory (called by extension methods from SuccessHound.Pagination package)
    /// </summary>
    public void SetPaginationFactory(object factory)
    {
        PaginationFactory = factory ?? throw new ArgumentNullException(nameof(factory));
    }
}
