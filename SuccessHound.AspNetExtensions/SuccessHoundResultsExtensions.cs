using Microsoft.AspNetCore.Http;
using SuccessHound.Abstractions;

namespace SuccessHound.AspNetExtensions;

/// <summary>
/// Minimal API extension methods for wrapping success responses with SuccessHound.
/// Provides common HTTP success helpers.
/// </summary>
public static class SuccessHoundResultsExtensions
{
    /// <summary>
    /// Returns a 200 OK response with data wrapped in SuccessHound.
    /// </summary>
    public static IResult Ok<T>(this T data, HttpContext context)
    {
        var formatter = GetFormatter(context);
        var wrapped = formatter.Format(data);
        return Results.Ok(wrapped);
    }

    /// <summary>
    /// Returns a 201 Created response with Location header and wrapped data.
    /// </summary>
    public static IResult Created<T>(this T data, string location, HttpContext context)
    {
        var formatter = GetFormatter(context);
        var wrapped = formatter.Format(data);
        return Results.Created(location, wrapped);
    }

    /// <summary>
    /// Returns a 204 No Content response.
    /// </summary>
    public static IResult NoContent()
    {
        return Results.NoContent();
    }

    /// <summary>
    /// Returns a 204 No Content response (helper for DELETE endpoints).
    /// </summary>
    public static IResult Deleted()
    {
        return Results.NoContent();
    }

    /// <summary>
    /// Returns a 200 OK response (helper for PUT/PATCH updates).
    /// </summary>
    public static IResult Updated<T>(this T data, HttpContext context)
    {
        var formatter = GetFormatter(context);
        var wrapped = formatter.Format(data);
        return Results.Ok(wrapped);
    }

    /// <summary>
    /// Returns a 200 OK response including metadata.
    /// </summary>
    public static IResult WithMeta<T>(this T data, object meta, HttpContext context)
    {
        var formatter = GetFormatter(context);
        var wrapped = formatter.Format(data, meta);
        return Results.Ok(wrapped);
    }

    /// <summary>
    /// Returns a response with a custom HTTP status code and wrapped data.
    /// </summary>
    public static IResult Custom<T>(this T data, int statusCode, HttpContext context)
    {
        var formatter = GetFormatter(context);
        var wrapped = formatter.Format(data);
        return Results.Json(wrapped, statusCode: statusCode);
    }

    private static ISuccessResponseFormatter GetFormatter(HttpContext context)
    {
        var formatter = context.RequestServices.GetService(typeof(ISuccessResponseFormatter)) as ISuccessResponseFormatter;
        if (formatter is null)
        {
            throw new InvalidOperationException(
                "ISuccessResponseFormatter is not registered. " +
                "Call builder.Services.AddSuccessHound(options => options.UseFormatter<T>()) in your Program.cs");
        }
        return formatter;
    }
}