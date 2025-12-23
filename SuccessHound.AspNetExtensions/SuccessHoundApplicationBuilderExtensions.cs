using Microsoft.AspNetCore.Builder;

namespace SuccessHound.AspNetExtensions;

/// <summary>
/// Extension methods for configuring SuccessHound middleware
/// </summary>
public static class SuccessHoundApplicationBuilderExtensions
{
    /// <summary>
    /// Adds SuccessHound middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseSuccessHound(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SuccessHoundMiddleware>();
    }
}
