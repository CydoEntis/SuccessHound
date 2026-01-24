using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using SuccessHound.Abstractions;
using SuccessHound.Defaults;
using SuccessHound.Pagination;
using SuccessHound.Pagination.Abstractions;
using SuccessHound.Pagination.Defaults;
using SuccessHound.Pagination.Extensions;
using SuccessHound.Pagination.Helpers;
using System.Text.Json;
using Xunit;

namespace SuccessHound.Tests
{
    public class PaginationTests
    {
        private readonly HttpContext _context;

        public PaginationTests()
        {
            // Configure services with pagination
            var services = new ServiceCollection();
            services.AddSingleton<ISuccessResponseFormatter, DefaultSuccessFormatter>();
            services.AddSingleton<IPaginationMetadataFactory, DefaultPaginationMetadataFactory>();
            var serviceProvider = services.BuildServiceProvider();

            _context = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_CreatesCorrectMetadata()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 2, pageSize: 10, totalCount: 95);

            Assert.Equal(2, metadata.Page);
            Assert.Equal(10, metadata.PageSize);
            Assert.Equal(95, metadata.TotalCount);
            Assert.Equal(10, metadata.TotalPages);
            Assert.True(metadata.HasNextPage);
            Assert.True(metadata.HasPreviousPage);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_FirstPage_HasNoPreviousPage()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 1, pageSize: 10, totalCount: 50);

            Assert.False(metadata.HasPreviousPage);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_LastPage_HasNoNextPage()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 5, pageSize: 10, totalCount: 50);

            Assert.False(metadata.HasNextPage);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_UnknownTotalCount_ReturnsNegativeOne()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 1, pageSize: 10, totalCount: -1);

            Assert.Equal(-1, metadata.TotalCount);
            Assert.Equal(-1, metadata.TotalPages);
        }

        [Fact]
        public void ToPagedResult_ReturnsCorrectPage()
        {
            var items = Enumerable.Range(1, 100).Select(i => new { Id = i, Name = $"Item {i}" }).ToList();

            var result = items.ToPagedResult(page: 2, pageSize: 10, _context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;
            var json = JsonSerializer.Serialize(okResult.Value);

            // Verify pagination metadata is present
            Assert.Contains("\"Page\":2", json);
            Assert.Contains("\"PageSize\":10", json);
            Assert.Contains("\"TotalCount\":100", json);
            Assert.Contains("\"Id\":11", json); // First item on page 2
            Assert.Contains("\"Id\":20", json); // Last item on page 2
        }

        [Fact]
        public void ToPagedResult_EmptyCollection_ReturnsEmptyPage()
        {
            var items = new List<string>();

            var result = items.ToPagedResult(page: 1, pageSize: 10, _context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;
            var json = JsonSerializer.Serialize(okResult.Value);

            Assert.Contains("\"TotalCount\":0", json);
            // When totalCount is 0, the factory should handle it appropriately
            // The implementation returns -1 for unknown/empty counts
        }

        [Fact]
        public void ToPagedResult_SinglePage_ReturnsCorrectly()
        {
            var items = new[] { "A", "B", "C" };

            var result = items.ToPagedResult(page: 1, pageSize: 10, _context);

            var okResult = (Ok<object>)result;
            var json = JsonSerializer.Serialize(okResult.Value);

            Assert.Contains("\"TotalCount\":3", json);
            Assert.Contains("\"TotalPages\":1", json);
            Assert.Contains("\"HasNextPage\":false", json);
        }

        [Fact]
        public void PaginationHelpers_NormalizePage_FixesNegativePage()
        {
            var normalized = PaginationHelpers.NormalizePage(-5);
            Assert.Equal(1, normalized);
        }

        [Fact]
        public void PaginationHelpers_NormalizePage_FixesZeroPage()
        {
            var normalized = PaginationHelpers.NormalizePage(0);
            Assert.Equal(1, normalized);
        }

        [Fact]
        public void PaginationHelpers_NormalizePage_KeepsValidPage()
        {
            var normalized = PaginationHelpers.NormalizePage(5);
            Assert.Equal(5, normalized);
        }

        [Fact]
        public void PaginationHelpers_NormalizePageSize_ClampsToMinimum()
        {
            var normalized = PaginationHelpers.NormalizePageSize(0);
            Assert.Equal(1, normalized);
        }

        [Fact]
        public void PaginationHelpers_NormalizePageSize_ClampsToMaximum()
        {
            var normalized = PaginationHelpers.NormalizePageSize(999);
            Assert.Equal(100, normalized);
        }

        [Fact]
        public void PaginationHelpers_NormalizePageSize_AllowsCustomRange()
        {
            var normalized = PaginationHelpers.NormalizePageSize(150, min: 10, max: 200);
            Assert.Equal(150, normalized);
        }

        [Fact]
        public void PaginationHelpers_Normalize_FixesBothParameters()
        {
            var (page, pageSize) = PaginationHelpers.Normalize(page: 0, pageSize: 999);

            Assert.Equal(1, page);
            Assert.Equal(100, pageSize);
        }

        [Fact]
        public void ConfigurationExtensions_UsePagination_WorksWithOptions()
        {
            // Test that UsePagination() can be called without throwing
            var options = new Options.SuccessHoundOptions();
            options.UseFormatter<DefaultSuccessFormatter>();
            options.UsePagination();

            // Verify pagination factory is set
            Assert.NotNull(options.PaginationFactory);
        }

        [Fact]
        public void ConfigurationExtensions_UsePaginationWithFactory_WorksWithCustomFactory()
        {
            var customFactory = new CustomTestPaginationFactory();
            var options = new Options.SuccessHoundOptions();
            options.UseFormatter<DefaultSuccessFormatter>();
            options.UsePagination(customFactory);

            // Verify pagination factory is set
            Assert.Equal(customFactory, options.PaginationFactory);
        }

        [Fact]
        public void ConfigurationExtensions_UsePaginationGeneric_WorksWithGenericType()
        {
            var options = new Options.SuccessHoundOptions();
            options.UseFormatter<DefaultSuccessFormatter>();
            options.UsePagination<CustomTestPaginationFactory>();

            // Verify pagination factory is set
            Assert.NotNull(options.PaginationFactory);
            Assert.IsType<CustomTestPaginationFactory>(options.PaginationFactory);
        }

        private class CustomTestPaginationFactory : IPaginationMetadataFactory
        {
            public PaginationMeta CreateMetadata(int page, int pageSize, int totalCount)
            {
                return new PaginationMeta
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : -1,
                    HasNextPage = totalCount > 0 && page < (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = page > 1
                };
            }
        }
    }
}
