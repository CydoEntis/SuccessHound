namespace SuccessHound.Pagination.Defaults;

/// <summary>
/// Strongly-typed pagination metadata for API responses.
/// </summary>
public class PaginationMeta
{
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages, or -1 if unknown
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages, or -1 if total count is unknown
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indicates whether there is a next page
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Indicates whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; init; }
}
