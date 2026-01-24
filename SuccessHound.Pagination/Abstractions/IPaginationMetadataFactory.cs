using SuccessHound.Pagination.Defaults;

namespace SuccessHound.Pagination.Abstractions;

/// <summary>
/// Defines how to create pagination metadata for API responses
/// </summary>
public interface IPaginationMetadataFactory
{
    /// <summary>
    /// Creates pagination metadata from pagination state
    /// </summary>
    /// <param name="page">Current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="totalCount">Total number of items across all pages, or -1 if unknown</param>
    /// <returns>Strongly-typed pagination metadata</returns>
    PaginationMeta CreateMetadata(int page, int pageSize, int totalCount);
}
