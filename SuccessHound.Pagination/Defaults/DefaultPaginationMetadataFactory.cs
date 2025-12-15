using SuccessHound.Pagination.Abstractions;

namespace SuccessHound.Pagination.Defaults;

/// <summary>
/// Default implementation of pagination metadata factory
/// </summary>
public class DefaultPaginationMetadataFactory : IPaginationMetadataFactory
{
    /// <summary>
    /// Creates standard pagination metadata
    /// </summary>
    public object CreateMetadata(int page, int pageSize, int totalCount)
    {
        var totalPages = totalCount > 0
            ? (int)Math.Ceiling(totalCount / (double)pageSize)
            : -1;

        return new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = totalCount > 0 && page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
