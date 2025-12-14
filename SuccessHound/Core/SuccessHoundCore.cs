using SuccessHound.Abstractions;

namespace SuccessHound
{
    /// <summary>
    /// Core entry point for SuccessHound — holds the configured factory and wraps data.
    /// </summary>
    public static class SuccessHoundCore
    {
        private static ISuccessResponseFactory? _factory;

        /// <summary>
        /// Configure SuccessHound with a user-provided response factory.
        /// </summary>
        public static void Configure(ISuccessResponseFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Wrap raw data using the configured factory.
        /// </summary>
        public static object Wrap(object? data)
        {
            if (_factory == null)
                throw new InvalidOperationException("SuccessHound is not configured. Call Configure() first.");

            return _factory.Wrap(data);
        }

        /// <summary>
        /// Wrap raw data with metadata using the configured factory.
        /// </summary>
        public static object Wrap(object? data, object? meta)
        {
            if (_factory == null)
                throw new InvalidOperationException("SuccessHound is not configured. Call Configure() first.");

            return _factory.Wrap(data, meta);
        }
    }
}