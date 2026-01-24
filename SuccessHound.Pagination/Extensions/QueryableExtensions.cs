using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SuccessHound.AspNetExtensions;
using SuccessHound.Pagination.Abstractions;
using SuccessHound.Pagination.Defaults;

namespace SuccessHound.Pagination.Extensions;

/// <summary>
/// Extension methods for paginating IQueryable sources (EF Core)
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paginated API response with SuccessHound wrapping
    /// </summary>
    /// <typeparam name="T">The type of items in the query</typeparam>
    /// <param name="query">The query to paginate (should already be filtered and ordered)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="context">HTTP context for accessing services</param>
    /// <param name="includeTotalCount">Whether to execute COUNT query (can be expensive for large tables)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IResult with paginated data wrapped in SuccessHound format</returns>
    /// <exception cref="InvalidOperationException">Thrown if pagination is not configured</exception>
    public static async Task<IResult> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        HttpContext context,
        bool includeTotalCount = true,
        CancellationToken cancellationToken = default)
    {
        var factory = context.RequestServices.GetService(typeof(IPaginationMetadataFactory)) as IPaginationMetadataFactory;
        if (factory is null)
        {
            throw new InvalidOperationException(
                "IPaginationMetadataFactory is not registered. " +
                "Call options.UsePagination() in AddSuccessHound configuration.");
        }

        var totalCount = includeTotalCount
            ? await query.CountAsync(cancellationToken)
            : -1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var metadata = factory.CreateMetadata(page, pageSize, totalCount);

        // Use strongly-typed WithMeta overload
        return items.WithMeta(metadata, context);
    }
}
