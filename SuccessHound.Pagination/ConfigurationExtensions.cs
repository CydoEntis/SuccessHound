using Microsoft.Extensions.DependencyInjection;
using SuccessHound.Options;
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
    /// <param name="options">SuccessHound options</param>
    /// <returns>Options instance for method chaining</returns>
    public static SuccessHoundOptions UsePagination(this SuccessHoundOptions options)
    {
        options.PaginationFactory = new DefaultPaginationMetadataFactory();
        return options;
    }

    /// <summary>
    /// Enable pagination with custom factory
    /// </summary>
    /// <param name="options">SuccessHound options</param>
    /// <param name="factory">Your custom pagination metadata factory</param>
    /// <returns>Options instance for method chaining</returns>
    public static SuccessHoundOptions UsePagination(
        this SuccessHoundOptions options,
        IPaginationMetadataFactory factory)
    {
        options.PaginationFactory = factory;
        return options;
    }

    /// <summary>
    /// Enable pagination with custom factory by type
    /// </summary>
    /// <typeparam name="TFactory">Factory type with parameterless constructor</typeparam>
    /// <param name="options">SuccessHound options</param>
    /// <returns>Options instance for method chaining</returns>
    public static SuccessHoundOptions UsePagination<TFactory>(this SuccessHoundOptions options)
        where TFactory : IPaginationMetadataFactory, new()
    {
        options.PaginationFactory = new TFactory();
        return options;
    }

    /// <summary>
    /// Registers pagination services with dependency injection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">SuccessHound options containing pagination configuration</param>
    internal static void AddPaginationServices(this IServiceCollection services, SuccessHoundOptions options)
    {
        if (options.PaginationFactory is not null)
        {
            services.AddSingleton(
                typeof(IPaginationMetadataFactory),
                options.PaginationFactory.GetType());
        }
    }
}
