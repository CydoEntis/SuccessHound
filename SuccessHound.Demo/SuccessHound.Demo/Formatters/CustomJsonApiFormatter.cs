using SuccessHound.Abstractions;

namespace SuccessHound.Demo.Formatters;

/// <summary>
/// Custom formatter that follows JSON:API specification format
/// </summary>
public sealed class CustomJsonApiFormatter : ISuccessResponseFormatter
{
    public object Format(object? data, object? meta = null)
    {
        return new
        {
            jsonapi = new { version = "1.0" },
            data,
            meta,
            links = new
            {
                self = "/api/current-endpoint" 
            }
        };
    }
}
