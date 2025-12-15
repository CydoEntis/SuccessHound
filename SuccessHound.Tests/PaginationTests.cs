using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    [Collection("SuccessHound Sequential")]
    public class PaginationTests
    {
        public PaginationTests()
        {
            // Configure SuccessHound with pagination
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination();
            });
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_CreatesCorrectMetadata()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 2, pageSize: 10, totalCount: 95);

            var json = JsonSerializer.Serialize(metadata);
            Assert.Contains("\"Page\":2", json);
            Assert.Contains("\"PageSize\":10", json);
            Assert.Contains("\"TotalCount\":95", json);
            Assert.Contains("\"TotalPages\":10", json);
            Assert.Contains("\"HasNextPage\":true", json);
            Assert.Contains("\"HasPreviousPage\":true", json);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_FirstPage_HasNoPreviousPage()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 1, pageSize: 10, totalCount: 50);

            var json = JsonSerializer.Serialize(metadata);
            Assert.Contains("\"HasPreviousPage\":false", json);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_LastPage_HasNoNextPage()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 5, pageSize: 10, totalCount: 50);

            var json = JsonSerializer.Serialize(metadata);
            Assert.Contains("\"HasNextPage\":false", json);
        }

        [Fact]
        public void DefaultPaginationMetadataFactory_UnknownTotalCount_ReturnsNegativeOne()
        {
            var factory = new DefaultPaginationMetadataFactory();

            var metadata = factory.CreateMetadata(page: 1, pageSize: 10, totalCount: -1);

            var json = JsonSerializer.Serialize(metadata);
            Assert.Contains("\"TotalCount\":-1", json);
            Assert.Contains("\"TotalPages\":-1", json);
        }

        [Fact]
        public void ToPagedResult_ReturnsCorrectPage()
        {
            var items = Enumerable.Range(1, 100).Select(i => new { Id = i, Name = $"Item {i}" }).ToList();

            var result = items.ToPagedResult(page: 2, pageSize: 10);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;
            var json = JsonSerializer.Serialize(okResult.Value);

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

            var result = items.ToPagedResult(page: 1, pageSize: 10);

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

            var result = items.ToPagedResult(page: 1, pageSize: 10);

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
        public void ConfigurationExtensions_UsePagination_WorksWithConfiguration()
        {
            // Test that UsePagination() can be called without throwing
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination();
            });

            // Verify pagination is enabled
            Assert.True(Core.SuccessHound.IsPaginationEnabled);
        }

        [Fact]
        public void ConfigurationExtensions_UsePaginationWithFactory_WorksWithCustomFactory()
        {
            var customFactory = new CustomTestPaginationFactory();

            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination(customFactory);
            });

            // Verify pagination is enabled
            Assert.True(Core.SuccessHound.IsPaginationEnabled);
        }

        [Fact]
        public void ConfigurationExtensions_UsePaginationGeneric_WorksWithGenericType()
        {
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination<CustomTestPaginationFactory>();
            });

            // Verify pagination is enabled
            Assert.True(Core.SuccessHound.IsPaginationEnabled);
        }

        [Fact]
        public void SuccessHound_GetPaginationFactory_ThrowsIfNotConfigured()
        {
            // Reset configuration
            typeof(Core.SuccessHound)
                .GetField("_paginationFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .SetValue(null, null);

            var exception = Assert.Throws<InvalidOperationException>(() => Core.SuccessHound.GetPaginationFactory());

            Assert.Contains("Pagination is not configured", exception.Message);

            // Restore for other tests
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination();
            });
        }

        [Fact]
        public void SuccessHound_IsPaginationEnabled_ReturnsTrueWhenConfigured()
        {
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination();
            });

            Assert.True(Core.SuccessHound.IsPaginationEnabled);
        }

        [Fact]
        public void SuccessHound_IsPaginationEnabled_ReturnsFalseWhenNotConfigured()
        {
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                // Don't call UsePagination()
            });

            Assert.False(Core.SuccessHound.IsPaginationEnabled);

            // Restore for other tests
            Core.SuccessHound.Configure(config =>
            {
                config.UseDefaultApiResponse();
                config.UsePagination();
            });
        }

        private class CustomTestPaginationFactory : IPaginationMetadataFactory
        {
            public object CreateMetadata(int page, int pageSize, int totalCount)
            {
                return new { CustomPage = page, CustomSize = pageSize };
            }
        }
    }
}
