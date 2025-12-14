namespace SuccessHound.Abstractions;

public interface ISuccessResponseFactory
{
    /// <summary>
    /// Wraps successful data in the user-defined response shape.
    /// </summary>
    /// <param name="data">The raw data returned by the handler</param>
    object Wrap(object data);
}