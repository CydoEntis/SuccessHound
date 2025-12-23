using Microsoft.Extensions.DependencyInjection;
using SuccessHound.Abstractions;
using SuccessHound.Options;

namespace SuccessHound.Extensions;

/// <summary>
/// Extension methods for configuring SuccessHound with dependency injection
/// </summary>
public static class SuccessHoundExtensions
{
    /// <summary>
    /// Adds SuccessHound services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for SuccessHound options</param>
    /// <returns>The service collection for method chaining</returns>
    /// <exception cref="InvalidOperationException">Thrown if formatter is not configured</exception>
    public static IServiceCollection AddSuccessHound(
        this IServiceCollection services,
        Action<SuccessHoundOptions> configure)
    {
        var options = new SuccessHoundOptions();
        configure(options);

        if (options.FormatterType is null)
            throw new InvalidOperationException(
                "SuccessHound requires a formatter. Call options.UseFormatter<T>() in the configuration.");

        services.AddSingleton(
            typeof(ISuccessResponseFormatter),
            options.FormatterType);
        
        if (options.PaginationFactory is not null)
        {
            var factoryType = options.PaginationFactory.GetType();
            services.AddSingleton(factoryType, options.PaginationFactory);
            
            var interfaceType = factoryType.GetInterfaces()
                .FirstOrDefault(i => i.Name.Contains("PaginationMetadataFactory"));
            if (interfaceType is not null)
            {
                services.AddSingleton(interfaceType, options.PaginationFactory);
            }
        }
        
        services.AddSingleton(options);

        return services;
    }
}
