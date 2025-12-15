using SuccessHound.Defaults;
using SuccessHound.Abstractions;
using Xunit;

namespace SuccessHound.Tests
{
    [Collection("SuccessHound Sequential")]
    public class SuccessHoundCoreTests
    {
        [Fact]
        public void Wrap_ReturnsApiResponseWithData()
        {
            SuccessHound.Configure(config => config.UseDefaultApiResponse());
            var payload = new { Name = "Cody" };

            var result = SuccessHound.Wrap(payload);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);

            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(payload, dataProp);
        }

        [Fact]
        public void Wrap_ThrowsIfNotConfigured()
        {
            typeof(SuccessHound)
                .GetField("_responseFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .SetValue(null, null);

            Assert.Throws<InvalidOperationException>(() => SuccessHound.Wrap(new { }));

            // Restore the factory for other tests
            SuccessHound.Configure(config => config.UseDefaultApiResponse());
        }

        [Fact]
        public void Wrap_WrapsCollectionCorrectly()
        {
            SuccessHound.Configure(config => config.UseDefaultApiResponse());
            var users = new List<object> { new { Name = "Alice" }, new { Name = "Bob" } };

            var result = SuccessHound.Wrap(users);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);

            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(users, dataProp);
        }

        [Fact]
        public void Wrap_WithMeta_IncludesMeta()
        {
            SuccessHound.Configure(config => config.UseDefaultApiResponse());
            var payload = new { Name = "Success Hound!" };
            var meta = new { Total = 1 };

            var result = SuccessHound.Wrap(payload, meta);

            var type = result.GetType();
            var dataProp = type.GetProperty("Data")!.GetValue(result);
            var metaProp = type.GetProperty("Meta")!.GetValue(result);

            Assert.Equal(payload, dataProp);
            Assert.Equal(meta, metaProp);
        }

        private class TestFactory : ISuccessResponseFactory
        {
            public object Wrap(object? data, object? meta = null) => new { Custom = data, Meta = meta };
        }

        [Fact]
        public void Wrap_UsesCustomFactory()
        {
            SuccessHound.Configure(config => config.UseApiResponse(new TestFactory()));
            var payload = new { Name = "Success Hound!" };
            var meta = new { Info = "Custom" };

            var result = SuccessHound.Wrap(payload, meta);

            var customProp = result.GetType().GetProperty("Custom")!.GetValue(result);
            var metaProp = result.GetType().GetProperty("Meta")!.GetValue(result);

            Assert.Equal(payload, customProp);
            Assert.Equal(meta, metaProp);
        }

        [Fact]
        public void Wrap_NullData_ReturnsApiResponse()
        {
            SuccessHound.Configure(config => config.UseDefaultApiResponse());
            object? payload = null;

            var result = SuccessHound.Wrap(payload);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);
            Assert.Null(type.GetProperty("Data")!.GetValue(result));
        }
    }
}