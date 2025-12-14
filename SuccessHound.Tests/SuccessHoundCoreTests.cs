using SuccessHound.Defaults;
using Xunit;
using SuccessHound.Abstractions;

namespace SuccessHound.Tests
{
    public class SuccessHoundCoreTests
    {
        [Fact]
        public void Wrap_ReturnsApiResponseWithData()
        {
            SuccessHoundCore.Configure(new DefaultApiResponseFactory());
            var payload = new { Name = "Cody" };

            var result = SuccessHoundCore.Wrap(payload);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);
            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(payload, dataProp);
        }

        [Fact]
        public void Wrap_ThrowsIfNotConfigured()
        {
            typeof(SuccessHoundCore)
                .GetField("_factory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .SetValue(null, null);

            Assert.Throws<InvalidOperationException>(() => SuccessHoundCore.Wrap(new { }));
        }

        [Fact]
        public void Wrap_WrapsCollectionCorrectly()
        {
            SuccessHoundCore.Configure(new DefaultApiResponseFactory());
            var users = new List<object> { new { Name = "Alice" }, new { Name = "Bob" } };

            var result = SuccessHoundCore.Wrap(users);

            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);
            var dataProp = type.GetProperty("Data")!.GetValue(result);
            Assert.Equal(users, dataProp);
        }

        [Fact]
        public void Wrap_WithMeta_IncludesMeta()
        {
            SuccessHoundCore.Configure(new DefaultApiResponseFactory());
            var payload = new { Name = "Success Hound!" };
            var meta = new { Total = 1 };

            var type = typeof(ApiResponse<>).MakeGenericType(payload.GetType());
            var okMethod = type.GetMethod("Ok")!;
            var wrapped = okMethod.Invoke(null, new object?[] { payload, meta });

            var metaProp = wrapped!.GetType().GetProperty("Meta")!.GetValue(wrapped);
            Assert.Equal(meta, metaProp);
        }

        private class TestFactory : ISuccessResponseFactory
        {
            public object Wrap(object data) => new { Custom = data };
        }

        [Fact]
        public void Wrap_UsesCustomFactory()
        {
            SuccessHoundCore.Configure(new TestFactory());
            var payload = new { Name = "Success Hound!" };

            var result = SuccessHoundCore.Wrap(payload);

            var customProp = result.GetType().GetProperty("Custom")!.GetValue(result);
            Assert.Equal(payload, customProp);
        }

        [Fact]
        public void Wrap_NullData_ReturnsApiResponse()
        {
            SuccessHoundCore.Configure(new DefaultApiResponseFactory());
            object? payload = null;

            var result = SuccessHoundCore.Wrap(payload);
            var type = result.GetType();
            Assert.Equal("ApiResponse`1", type.Name);
            Assert.Null(type.GetProperty("Data")!.GetValue(result));
        }
    }
}