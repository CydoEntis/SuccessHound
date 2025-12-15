using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SuccessHound.AspNetExtensions;

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
    /// <param name="includeTotalCount">Whether to execute COUNT query (can be expensive for large tables)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>IResult with paginated data wrapped in SuccessHound format</returns>
    /// <exception cref="InvalidOperationException">Thrown if pagination is not configured</exception>
    public static async Task<IResult> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        bool includeTotalCount = true,
        CancellationToken cancellationToken = default)
    {
        var factory = Core.SuccessHound.GetPaginationFactory();

        var totalCount = includeTotalCount
            ? await query.CountAsync(cancellationToken)
            : -1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var metadata = factory.CreateMetadata(page, pageSize, totalCount);

        return items.WithMeta(new { Pagination = metadata });
    }
}
