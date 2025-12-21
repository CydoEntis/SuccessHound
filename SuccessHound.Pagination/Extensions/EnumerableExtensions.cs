using Microsoft.AspNetCore.Http;
using SuccessHound.AspNetExtensions;
using SuccessHound.Pagination.Abstractions;

namespace SuccessHound.Pagination.Extensions;

/// <summary>
/// Extension methods for paginating in-memory collections
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Converts an IEnumerable to a paginated API response with SuccessHound wrapping
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="source">The source collection to paginate</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="context">HTTP context for accessing services</param>
    /// <returns>IResult with paginated data wrapped in SuccessHound format</returns>
    /// <exception cref="InvalidOperationException">Thrown if pagination is not configured</exception>
    public static IResult ToPagedResult<T>(
        this IEnumerable<T> source,
        int page,
        int pageSize,
        HttpContext context)
    {
        var factory = context.RequestServices.GetService(typeof(IPaginationMetadataFactory)) as IPaginationMetadataFactory;
        if (factory is null)
        {
            throw new InvalidOperationException(
                "IPaginationMetadataFactory is not registered. " +
                "Call options.UsePagination() in AddSuccessHound configuration.");
        }

        var sourceList = source as IList<T> ?? source.ToList();

        var totalCount = sourceList.Count;

        var items = sourceList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var metadata = factory.CreateMetadata(page, pageSize, totalCount);

        return items.WithMeta(new { Pagination = metadata }, context);
    }
}
