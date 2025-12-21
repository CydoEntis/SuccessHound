using SuccessHound.Defaults;
using SuccessHound.Abstractions;
using Xunit;

namespace SuccessHound.Tests
{
    public class SuccessHoundCoreTests
    {
        [Fact]
        public void Format_ReturnsApiResponseWithData()
        {
            var formatter = new DefaultSuccessFormatter();
            var payload = new { Name = "Cody" };

            var result = formatter.Format(payload);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);

            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(payload, dataProp);
        }

        [Fact]
        public void Format_WrapsCollectionCorrectly()
        {
            var formatter = new DefaultSuccessFormatter();
            var users = new List<object> { new { Name = "Alice" }, new { Name = "Bob" } };

            var result = formatter.Format(users);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);

            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(users, dataProp);
        }

        [Fact]
        public void Format_WithMeta_IncludesMeta()
        {
            var formatter = new DefaultSuccessFormatter();
            var payload = new { Name = "Success Hound!" };
            var meta = new { Total = 1 };

            var result = formatter.Format(payload, meta);

            var type = result.GetType();
            var dataProp = type.GetProperty("Data")!.GetValue(result);
            var metaProp = type.GetProperty("Meta")!.GetValue(result);

            Assert.Equal(payload, dataProp);
            Assert.Equal(meta, metaProp);
        }

        private class TestFormatter : ISuccessResponseFormatter
        {
            public object Format(object? data, object? meta = null) => new { Custom = data, Meta = meta };
        }

        [Fact]
        public void Format_UsesCustomFormatter()
        {
            var formatter = new TestFormatter();
            var payload = new { Name = "Success Hound!" };
            var meta = new { Info = "Custom" };

            var result = formatter.Format(payload, meta);

            var customProp = result.GetType().GetProperty("Custom")!.GetValue(result);
            var metaProp = result.GetType().GetProperty("Meta")!.GetValue(result);

            Assert.Equal(payload, customProp);
            Assert.Equal(meta, metaProp);
        }

        [Fact]
        public void Format_NullData_ReturnsApiResponse()
        {
            var formatter = new DefaultSuccessFormatter();
            object? payload = null;

            var result = formatter.Format(payload);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);
            Assert.Null(type.GetProperty("Data")!.GetValue(result));
        }
    }
}