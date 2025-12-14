using SuccessHound.Abstractions;

namespace SuccessHound.Core;

public static class SuccessHound
{
    private static ISuccessResponseFactory? _factory;

    /// <summary>
    /// Configures SuccessHound with a user-provided response factory.
    /// </summary>
    public static void Configure(ISuccessResponseFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Wraps raw data using the configured factory.
    /// </summary>
    internal static object Wrap(object data)
    {
        if (_factory == null)
            throw new InvalidOperationException("SuccessHound is not configured. Call SuccessHound.Configure() first.");

        return _factory.Wrap(data);
    }
}