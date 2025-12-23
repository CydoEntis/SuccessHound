using Microsoft.AspNetCore.Http;

namespace SuccessHound.AspNetExtensions;

/// <summary>
/// Optional middleware for SuccessHound.
/// Currently a pass-through, but can be extended for cross-cutting concerns.
/// </summary>
public sealed class SuccessHoundMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuccessHoundMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    public SuccessHoundMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
