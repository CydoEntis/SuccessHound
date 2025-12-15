namespace SuccessHound.Pagination.Helpers;

/// <summary>
/// Optional helpers for pagination parameter normalization
/// </summary>
public static class PaginationHelpers
{
    /// <summary>
    /// Normalizes page number to be at least 1
    /// </summary>
    /// <param name="page">Page number from user input</param>
    /// <returns>Normalized page number (minimum 1)</returns>
    public static int NormalizePage(int page) => Math.Max(1, page);

    /// <summary>
    /// Clamps page size between minimum and maximum values
    /// </summary>
    /// <param name="pageSize">Page size from user input</param>
    /// <param name="min">Minimum allowed page size (default: 1)</param>
    /// <param name="max">Maximum allowed page size (default: 100)</param>
    /// <returns>Normalized page size within bounds</returns>
    public static int NormalizePageSize(int pageSize, int min = 1, int max = 100)
        => Math.Clamp(pageSize, min, max);

    /// <summary>
    /// Normalizes both page and page size in one call
    /// </summary>
    /// <param name="page">Page number from user input</param>
    /// <param name="pageSize">Page size from user input</param>
    /// <param name="minPageSize">Minimum allowed page size (default: 1)</param>
    /// <param name="maxPageSize">Maximum allowed page size (default: 100)</param>
    /// <returns>Tuple of (normalized page, normalized pageSize)</returns>
    public static (int page, int pageSize) Normalize(
        int page,
        int pageSize,
        int minPageSize = 1,
        int maxPageSize = 100)
    {
        return (
            NormalizePage(page),
            NormalizePageSize(pageSize, minPageSize, maxPageSize)
        );
    }
}
