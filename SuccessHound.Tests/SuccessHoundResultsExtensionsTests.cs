using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using SuccessHound.Abstractions;
using SuccessHound.AspNetExtensions;
using SuccessHound.Defaults;
using System.Text.Json;
using Xunit;

namespace SuccessHound.Tests
{
    public class SuccessHoundResultsExtensionsTests
    {
        private readonly HttpContext _context;

        public SuccessHoundResultsExtensionsTests()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ISuccessResponseFormatter, DefaultSuccessFormatter>();
            var serviceProvider = services.BuildServiceProvider();

            _context = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
        }

        [Fact]
        public void Ok_ReturnsOkResultWithWrappedData()
        {
            var payload = new { Name = "Test User" };

            var result = payload.Ok(_context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;

            var json = JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Name\":\"Test User\"", json);
        }

        [Fact]
        public void Created_ReturnsCreatedResultWithLocation()
        {
            var payload = new { Id = 123, Name = "New Item" };
            var location = "/api/items/123";

            var result = payload.Created(location, _context);

            Assert.IsType<Created<object>>(result);
            var createdResult = (Created<object>)result;
            Assert.Equal(location, createdResult.Location);

            var json = JsonSerializer.Serialize(createdResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Id\":123", json);
        }

        [Fact]
        public void NoContent_ReturnsNoContentResult()
        {
            var result = SuccessHoundResultsExtensions.NoContent();

            Assert.IsType<NoContent>(result);
        }

        [Fact]
        public void Deleted_ReturnsNoContentResult()
        {
            var result = SuccessHoundResultsExtensions.Deleted();

            Assert.IsType<NoContent>(result);
        }

        [Fact]
        public void Updated_ReturnsOkResultWithWrappedData()
        {
            var payload = new { Id = 456, Name = "Updated Item" };

            var result = payload.Updated(_context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;

            var json = JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Id\":456", json);
        }

        [Fact]
        public void WithMeta_ReturnsOkResultWithMetadata()
        {
            var payload = new { Name = "Item" };
            var meta = new { Total = 100, Page = 1 };

            var result = payload.WithMeta(meta, _context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;

            var json = JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Name\":\"Item\"", json);
            Assert.Contains("\"Total\":100", json);
            Assert.Contains("\"Page\":1", json);
        }

        [Fact]
        public void Custom_ReturnsJsonResultWithCustomStatusCode()
        {
            var payload = new { Message = "Custom response" };
            var statusCode = 202;

            var result = payload.Custom(statusCode, _context);

            Assert.IsType<JsonHttpResult<object>>(result);
            var jsonResult = (JsonHttpResult<object>)result;
            Assert.Equal(statusCode, jsonResult.StatusCode);

            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Message\":\"Custom response\"", json);
        }

        [Fact]
        public void Ok_WithNullData_ReturnsWrappedNull()
        {
            object? payload = null;

            var result = payload.Ok(_context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;

            var json = JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Data\":null", json);
        }

        [Fact]
        public void Ok_WithCollection_ReturnsWrappedCollection()
        {
            var payload = new List<object>
            {
                new { Id = 1, Name = "Item 1" },
                new { Id = 2, Name = "Item 2" }
            };

            var result = payload.Ok(_context);

            Assert.IsType<Ok<object>>(result);
            var okResult = (Ok<object>)result;

            var json = JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("\"Success\":true", json);
            Assert.Contains("\"Item 1\"", json);
            Assert.Contains("\"Item 2\"", json);
        }
    }
}
