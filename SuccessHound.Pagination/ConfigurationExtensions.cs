using SuccessHound.Config;
using SuccessHound.Pagination.Abstractions;
using SuccessHound.Pagination.Defaults;

namespace SuccessHound.Pagination;

/// <summary>
/// Extension methods to add pagination configuration to SuccessHound
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Enable pagination with default factory
    /// </summary>
    /// <param name="config">SuccessHound configuration</param>
    /// <returns>Configuration instance for method chaining</returns>
    public static SuccessHoundConfiguration UsePagination(this SuccessHoundConfiguration config)
    {
        config.SetPaginationFactory(new DefaultPaginationMetadataFactory());
        return config;
    }

    /// <summary>
    /// Enable pagination with custom factory
    /// </summary>
    /// <param name="config">SuccessHound configuration</param>
    /// <param name="factory">Your custom pagination metadata factory</param>
    /// <returns>Configuration instance for method chaining</returns>
    public static SuccessHoundConfiguration UsePagination(
        this SuccessHoundConfiguration config,
        IPaginationMetadataFactory factory)
    {
        config.SetPaginationFactory(factory);
        return config;
    }

    /// <summary>
    /// Enable pagination with custom factory by type
    /// </summary>
    /// <typeparam name="TFactory">Factory type with parameterless constructor</typeparam>
    /// <param name="config">SuccessHound configuration</param>
    /// <returns>Configuration instance for method chaining</returns>
    public static SuccessHoundConfiguration UsePagination<TFactory>(this SuccessHoundConfiguration config)
        where TFactory : IPaginationMetadataFactory, new()
    {
        config.SetPaginationFactory(new TFactory());
        return config;
    }
}
