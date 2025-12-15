using Microsoft.AspNetCore.Http;

namespace SuccessHound.AspNetExtensions
{
    /// <summary>
    /// Minimal API extension methods for wrapping success responses with SuccessHoundCore.
    /// Provides common HTTP success helpers.
    /// </summary>
    public static class SuccessHoundResultsExtensions
    {
        /// <summary>
        /// Returns a 200 OK response with data wrapped in SuccessHound.
        /// </summary>
        public static IResult Ok<T>(this T data)
        {
            var wrapped = SuccessHound.Wrap(data);
            return Results.Ok(wrapped);
        }

        /// <summary>
        /// Returns a 201 Created response with Location header and wrapped data.
        /// </summary>
        public static IResult Created<T>(this T data, string location)
        {
            var wrapped = SuccessHound.Wrap(data);
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
        public static IResult Updated<T>(this T data)
        {
            var wrapped = SuccessHound.Wrap(data);
            return Results.Ok(wrapped);
        }

        /// <summary>
        /// Returns a 200 OK response including metadata.
        /// </summary>
        public static IResult WithMeta<T>(this T data, object meta)
        {
            var wrapped = SuccessHound.Wrap(data, meta);
            return Results.Ok(wrapped);
        }

        /// <summary>
        /// Returns a response with a custom HTTP status code and wrapped data.
        /// </summary>
        public static IResult Custom<T>(this T data, int statusCode)
        {
            var wrapped = SuccessHound.Wrap(data);
            return Results.Json(wrapped, statusCode: statusCode);
        }
    }
}