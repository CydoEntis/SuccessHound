# SuccessHound

<p align="center">
  <img src="assets/logo.png" alt="SuccessHound Logo" style="width: 180px; border-radius: 20px;"/>
</p>

<p align="center">
  <em>A lightweight, extensible .NET library for wrapping API responses in consistent success envelopes.</em>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/SuccessHound"><img src="https://img.shields.io/nuget/v/SuccessHound?style=flat-square&logo=nuget&label=SuccessHound" alt="NuGet Version"></a>
  <a href="https://www.nuget.org/packages/SuccessHound.AspNetExtensions"><img src="https://img.shields.io/nuget/v/SuccessHound.AspNetExtensions?style=flat-square&logo=nuget&label=AspNetExtensions" alt="NuGet Version"></a>
  <a href="https://www.nuget.org/packages/SuccessHound.Pagination"><img src="https://img.shields.io/nuget/v/SuccessHound.Pagination?style=flat-square&logo=nuget&label=Pagination" alt="NuGet Version"></a>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/SuccessHound"><img src="https://img.shields.io/nuget/dt/SuccessHound?style=flat-square&logo=nuget&label=downloads&color=blue" alt="NuGet Downloads"></a>
  <a href="https://github.com/CydoEntis/SuccessHound/blob/master/LICENSE"><img src="https://img.shields.io/badge/license-MIT-green?style=flat-square" alt="License"></a>
  <a href="https://dotnet.microsoft.com/download"><img src="https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet" alt=".NET Version"></a>
  <a href="https://github.com/CydoEntis/SuccessHound"><img src="https://img.shields.io/github/stars/CydoEntis/SuccessHound?style=flat-square&logo=github" alt="GitHub Stars"></a>
</p>

## Why SuccessHound?

- **Consistent API responses** - Standardized success envelope across your entire API
- **Strongly-typed metadata** - Full type safety with `ApiResponse<TData, TMeta>` - no runtime casting
- **Swagger/OpenAPI ready** - Proper schema generation for all metadata types
- **Extensible** - Factory pattern lets you customize everything
- **Backward compatible** - Existing code continues to work with `ApiResponse<T>`
- **Optional features** - Only install what you need (core + pagination)
- **Framework-agnostic** - Core works anywhere, extensions are optional
- **Zero ceremony** - Fluent API with minimal configuration

## Packages

| Package | Description | NuGet | When to Use |
|---------|-------------|-------|-------------|
| **SuccessHound** | Core response wrapping (framework-agnostic) | [![NuGet](https://img.shields.io/nuget/v/SuccessHound?style=flat-square)](https://www.nuget.org/packages/SuccessHound) | Required - Install first |
| **SuccessHound.AspNetExtensions** | ASP.NET Core Minimal API extensions | [![NuGet](https://img.shields.io/nuget/v/SuccessHound.AspNetExtensions?style=flat-square)](https://www.nuget.org/packages/SuccessHound.AspNetExtensions) | Required for ASP.NET Core |
| **SuccessHound.Pagination** | EF Core + in-memory pagination | [![NuGet](https://img.shields.io/nuget/v/SuccessHound.Pagination?style=flat-square)](https://www.nuget.org/packages/SuccessHound.Pagination) | Optional - Only if you need pagination |

## Installation

### Basic Setup (No Pagination)

```bash
dotnet add package SuccessHound
dotnet add package SuccessHound.AspNetExtensions
```

### With Pagination

```bash
dotnet add package SuccessHound
dotnet add package SuccessHound.AspNetExtensions
dotnet add package SuccessHound.Pagination
```

## Quick Start

### Minimal Setup

```csharp
using SuccessHound.Extensions;
using SuccessHound.Defaults;

var builder = WebApplication.CreateBuilder(args);

// Configure SuccessHound with DI
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<DefaultSuccessFormatter>();
});

var app = builder.Build();

// Optional: Add middleware (currently pass-through)
app.UseSuccessHound();

app.MapGet("/users/{id}", (int id, HttpContext context) =>
{
    var user = new { Id = id, Name = "John Doe", Email = "john@example.com" };
    return user.Ok(context); // Wraps in success envelope
});

app.Run();
```

**Response:**

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com"
  },
  "meta": null,
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### With Pagination

```csharp
using SuccessHound.Extensions;
using SuccessHound.Defaults;
using SuccessHound.Pagination;
using SuccessHound.Pagination.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure SuccessHound with pagination
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<DefaultSuccessFormatter>();
    options.UsePagination(); // Add this line!
});

var app = builder.Build();

app.UseSuccessHound();

app.MapGet("/users", async (AppDbContext db, HttpContext context, int page = 1, int pageSize = 10) =>
{
    return await db.Users
        .OrderBy(u => u.Id)
        .ToPagedResultAsync(page, pageSize, context);
});

app.Run();
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Alice"
    },
    {
      "id": 2,
      "name": "Bob"
    }
  ],
  "meta": {
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalCount": 100,
      "totalPages": 10,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  },
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

## Response Structure

SuccessHound uses a strongly-typed response envelope system:

### Core Types

```csharp
// Two-generic envelope for responses with metadata
public class ApiResponse<TData, TMeta>
{
    public bool Success { get; init; }      // Always true for success responses
    public TData? Data { get; init; }       // Your payload
    public TMeta? Meta { get; init; }       // Strongly-typed metadata
    public DateTime Timestamp { get; init; } // UTC timestamp
}

// Backward-compatible wrapper for responses without metadata
public class ApiResponse<T> : ApiResponse<T, NoMeta>
{
    // Inherits all properties, Meta is always NoMeta.Instance
}
```

### Benefits of Strongly-Typed Metadata

- **Type Safety**: Compile-time checking for metadata types
- **Swagger/OpenAPI**: Proper schema generation for metadata
- **IntelliSense**: Full IDE support when working with metadata
- **No Runtime Casting**: Direct access to typed metadata properties

## Extension Methods

### `.Ok<T>()`

Returns `200 OK` with wrapped data.

```csharp
app.MapGet("/products/{id}", (int id, HttpContext context) =>
{
    var product = GetProduct(id);
    return product.Ok(context);
});
```

**Response:** `200 OK`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Product Name"
  },
  "meta": null,
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### `.Created<T>(string location)`

Returns `201 Created` with Location header.

```csharp
app.MapPost("/products", (Product product, HttpContext context) =>
{
    var created = CreateProduct(product);
    return created.Created($"/products/{created.Id}", context);
});
```

**Response:** `201 Created` with `Location: /products/123` header

### `.Updated<T>()`

Returns `200 OK` for update operations.

```csharp
app.MapPut("/products/{id}", (int id, Product product, HttpContext context) =>
{
    var updated = UpdateProduct(id, product);
    return updated.Updated(context);
});
```

**Response:** `200 OK` with wrapped data

### `.NoContent()` / `.Deleted()`

Returns `204 No Content` for delete operations.

```csharp
app.MapDelete("/products/{id}", (int id) =>
{
    DeleteProduct(id);
    return SuccessHoundResultsExtensions.Deleted();
});
```

**Response:** `204 No Content` (no body)

### `.WithMeta<TData, TMeta>(TMeta meta)` (Strongly-Typed)

Returns `200 OK` with strongly-typed metadata. **Recommended for new code.**

```csharp
// Define your metadata type
public class VersionMeta
{
    public string Version { get; init; } = "v1.0";
    public DateTime ServerTime { get; init; } = DateTime.UtcNow;
}

app.MapGet("/products", (HttpContext context, int page = 1) =>
{
    var products = GetProducts(page);
    var meta = new VersionMeta { Version = "v2.0" };
    return products.WithMeta(meta, context);
});
```

**Response:**

```json
{
  "success": true,
  "data": [
    ...
  ],
  "meta": {
    "version": "v2.0",
    "serverTime": "2025-12-15T10:30:00.000Z"
  },
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### `.WithMeta<T>(object meta)` (Backward-Compatible)

Returns `200 OK` with custom metadata. **Still supported for backward compatibility.**

```csharp
app.MapGet("/products", (HttpContext context, int page = 1) =>
{
    var products = GetProducts(page);
    var meta = new
    {
        Page = page,
        Version = "v1.0",
        ServerTime = DateTime.UtcNow
    };
    return products.WithMeta(meta, context);
});
```

### `.Custom<T>(int statusCode)`

Returns custom HTTP status code.

```csharp
app.MapPost("/products/process", (Product product, HttpContext context) =>
{
    var result = ProcessProduct(product);
    return result.Custom(202, context); // 202 Accepted
});
```

**Response:** `202 Accepted` with wrapped data

## Pagination

### EF Core Pagination

```csharp
using SuccessHound.Pagination.Extensions;

app.MapGet("/users", async (AppDbContext db, HttpContext context, int page = 1, int pageSize = 20) =>
{
    return await db.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .ToPagedResultAsync(page, pageSize, context);
});
```

### In-Memory Pagination

```csharp
app.MapGet("/items", (HttpContext context, int page = 1, int pageSize = 10) =>
{
    var items = GetAllItems(); // Returns IEnumerable<T>
    return items.ToPagedResult(page, pageSize, context);
});
```

### Pagination Metadata

Pagination now uses strongly-typed `PaginationMeta`:

```csharp
public class PaginationMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
```

**Response:**

```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Alice" },
    { "id": 2, "name": "Bob" }
  ],
  "meta": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### Strongly-Typed Pagination Usage

You can also use pagination with explicit types:

```csharp
using SuccessHound.Defaults;
using SuccessHound.Pagination.Defaults;

app.MapGet("/users", async (AppDbContext db, HttpContext context, int page = 1, int pageSize = 10) =>
{
    var users = await db.Users
        .OrderBy(u => u.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    var totalCount = await db.Users.CountAsync();
    var factory = context.RequestServices.GetRequiredService<IPaginationMetadataFactory>();
    var meta = factory.CreateMetadata(page, pageSize, totalCount);
    
    // Explicitly typed response
    return Results.Ok(ApiResponse<IReadOnlyList<User>, PaginationMeta>.Ok(users, meta));
});
```

## Advanced Usage

### Custom Response Formatter

Create your own response structure:

```csharp
using SuccessHound.Abstractions;

public sealed class MyCustomFormatter : ISuccessResponseFormatter
{
    public object Format(object? data, object? meta = null)
    {
        return new
        {
            Status = "success",
            Result = data,
            Metadata = meta,
            Version = "v2.0",
            Timestamp = DateTime.UtcNow
        };
    }
}
```

Use it:

```csharp
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<MyCustomFormatter>();
});
```

### Custom Pagination Factory

Customize pagination metadata by implementing `IPaginationMetadataFactory`:

```csharp
using SuccessHound.Pagination.Abstractions;
using SuccessHound.Pagination.Defaults;

public sealed class MyPaginationFactory : IPaginationMetadataFactory
{
    public PaginationMeta CreateMetadata(int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Return strongly-typed PaginationMeta
        return new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
```

Use it:

```csharp
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<DefaultSuccessFormatter>();
    options.UsePagination(new MyPaginationFactory());
});
```

**Note**: For custom metadata structures beyond `PaginationMeta`, you can create your own metadata types and use `ApiResponse<TData, TMeta>` directly.

### Framework-Agnostic Usage

Use SuccessHound formatters outside of ASP.NET Core:

```csharp
using SuccessHound.Abstractions;
using SuccessHound.Defaults;

// Create formatter
var formatter = new DefaultSuccessFormatter();

// Format data
var data = new { Message = "Hello, World!" };
var wrapped = formatter.Format(data);

// With metadata
var meta = new { Version = "1.0" };
var wrappedWithMeta = formatter.Format(data, meta);
```

## Common Scenarios

### Handling Null Data

SuccessHound handles null data gracefully:

```csharp
app.MapGet("/user/{id}", (int id, HttpContext context) =>
{
    var user = FindUser(id); // May return null
    return user.Ok(context);
});
```

**Response:**

```json
{
  "success": true,
  "data": null,
  "meta": null,
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### Collections

```csharp
app.MapGet("/users", (HttpContext context) =>
{
    var users = new List<User>
    {
        new User { Id = 1, Name = "Alice" },
        new User { Id = 2, Name = "Bob" }
    };
    return users.Ok(context);
});
```

**Response:**

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Alice"
    },
    {
      "id": 2,
      "name": "Bob"
    }
  ],
  "meta": null,
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### Complex Metadata

```csharp
app.MapGet("/report", (HttpContext context) =>
{
    var report = GenerateReport();
    var meta = new
    {
        GeneratedAt = DateTime.UtcNow,
        GeneratedBy = "System",
        Format = "JSON",
        Version = "1.0",
        Filters = new { StartDate = "2025-01-01", EndDate = "2025-12-31" }
    };
    return report.WithMeta(meta, context);
});
```

## Requirements

- **.NET 8.0** or higher
- **ASP.NET Core 8.0+** (for AspNetExtensions package)
- **Entity Framework Core 8.0+** (for `ToPagedResultAsync()` in Pagination package)

## License

MIT License - See [LICENSE](LICENSE) for details.

**Built with care for clean, consistent API responses.**
