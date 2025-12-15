# SuccessHound

<p align="center">
  <img src="assets/logo.png" alt="SuccessHound Logo" style="width: 180px; border-radius: 20px;"/>
</p>

<p align="center">
  <em>A lightweight, extensible .NET library for wrapping API responses in consistent success envelopes.</em>
</p>

## Why SuccessHound?

- **Consistent API responses** - Standardized success envelope across your entire API
- **Type-safe** - Full generic support with IntelliSense
- **Extensible** - Factory pattern lets you customize everything
- **Optional features** - Only install what you need (core + pagination)
- **Framework-agnostic** - Core works anywhere, extensions are optional
- **Zero ceremony** - Fluent API with minimal configuration

## Packages

| Package                           | Description                                 | When to Use                            |
|-----------------------------------|---------------------------------------------|----------------------------------------|
| **SuccessHound**                  | Core response wrapping (framework-agnostic) | Required - Install first               |
| **SuccessHound.AspNetExtensions** | ASP.NET Core Minimal API extensions         | Required for ASP.NET Core              |
| **SuccessHound.Pagination**       | EF Core + in-memory pagination              | Optional - Only if you need pagination |

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
using SuccessHound;

var builder = WebApplication.CreateBuilder(args);

// Configure SuccessHound
SuccessHound.Configure(config =>
{
    config.UseDefaultApiResponse();
});

var app = builder.Build();

app.MapGet("/users/{id}", (int id) =>
{
    var user = new { Id = id, Name = "John Doe", Email = "john@example.com" };
    return user.Ok(); // Wraps in success envelope
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
using SuccessHound;
using SuccessHound.Pagination;
using SuccessHound.Pagination.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Enable pagination
SuccessHound.Configure(config =>
{
    config.UseDefaultApiResponse();
    config.UsePagination(); // Add this line!
});

var app = builder.Build();

app.MapGet("/users", async (AppDbContext db, int page = 1, int pageSize = 10) =>
{
    return await db.Users
        .OrderBy(u => u.Id)
        .ToPagedResultAsync(page, pageSize);
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

All responses wrapped by SuccessHound include:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; init; }      // Always true for success responses
    public T? Data { get; init; }           // Your payload
    public object? Meta { get; init; }      // Optional metadata (pagination, etc.)
    public DateTime Timestamp { get; init; } // UTC timestamp
}
```

## Extension Methods

### `.Ok<T>()`

Returns `200 OK` with wrapped data.

```csharp
app.MapGet("/products/{id}", (int id) =>
{
    var product = GetProduct(id);
    return product.Ok();
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
app.MapPost("/products", (Product product) =>
{
    var created = CreateProduct(product);
    return created.Created($"/products/{created.Id}");
});
```

**Response:** `201 Created` with `Location: /products/123` header

### `.Updated<T>()`

Returns `200 OK` for update operations.

```csharp
app.MapPut("/products/{id}", (int id, Product product) =>
{
    var updated = UpdateProduct(id, product);
    return updated.Updated();
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

### `.WithMeta<T>(object meta)`

Returns `200 OK` with custom metadata.

```csharp
app.MapGet("/products", (int page = 1) =>
{
    var products = GetProducts(page);
    var meta = new
    {
        Page = page,
        Version = "v1.0",
        ServerTime = DateTime.UtcNow
    };
    return products.WithMeta(meta);
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
    "page": 1,
    "version": "v1.0",
    "serverTime": "2025-12-15T10:30:00.000Z"
  },
  "timestamp": "2025-12-15T10:30:00.000Z"
}
```

### `.Custom<T>(int statusCode)`

Returns custom HTTP status code.

```csharp
app.MapPost("/products/process", (Product product) =>
{
    var result = ProcessProduct(product);
    return result.Custom(202); // 202 Accepted
});
```

**Response:** `202 Accepted` with wrapped data

## Pagination

### EF Core Pagination

```csharp
using SuccessHound.Pagination.Extensions;

app.MapGet("/users", async (AppDbContext db, int page = 1, int pageSize = 20) =>
{
    return await db.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .ToPagedResultAsync(page, pageSize);
});
```

### In-Memory Pagination

```csharp
app.MapGet("/items", (int page = 1, int pageSize = 10) =>
{
    var items = GetAllItems(); // Returns IEnumerable<T>
    return items.ToPagedResult(page, pageSize);
});
```

### Pagination Metadata

Default pagination metadata includes:

```json
{
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

## Advanced Usage

### Custom Response Factory

Create your own response structure:

```csharp
using SuccessHound.Abstractions;

public class MyApiResponseFactory : ISuccessResponseFactory
{
    public object Wrap(object? data, object? meta = null)
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
SuccessHound.Configure(config =>
{
    config.UseApiResponse(new MyApiResponseFactory());
});
```

### Custom Pagination Factory

Customize pagination metadata:

```csharp
using SuccessHound.Pagination.Abstractions;

public class MyPaginationFactory : IPaginationMetadataFactory
{
    public object CreateMetadata(int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new
        {
            CurrentPage = page,
            PerPage = pageSize,
            Total = totalCount,
            TotalPages = totalPages,
            Links = new
            {
                Next = page < totalPages ? $"/api?page={page + 1}" : null,
                Previous = page > 1 ? $"/api?page={page - 1}" : null
            }
        };
    }
}
```

Use it:

```csharp
SuccessHound.Configure(config =>
{
    config.UseDefaultApiResponse();
    config.UsePagination(new MyPaginationFactory());
});
```

### Framework-Agnostic Usage

Use SuccessHound outside of ASP.NET Core:

```csharp
using SuccessHound;
using SuccessHound.Defaults;

// Configure
SuccessHoundCore.Configure(new DefaultApiResponseFactory());

// Wrap data
var data = new { Message = "Hello, World!" };
var wrapped = SuccessHoundCore.Wrap(data);

// With metadata
var meta = new { Version = "1.0" };
var wrappedWithMeta = SuccessHoundCore.Wrap(data, meta);
```

## Common Scenarios

### Handling Null Data

SuccessHound handles null data gracefully:

```csharp
app.MapGet("/user/{id}", (int id) =>
{
    var user = FindUser(id); // May return null
    return user.Ok();
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
app.MapGet("/users", () =>
{
    var users = new List<User>
    {
        new User { Id = 1, Name = "Alice" },
        new User { Id = 2, Name = "Bob" }
    };
    return users.Ok();
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
app.MapGet("/report", () =>
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
    return report.WithMeta(meta);
});
```

## Requirements

- **.NET 8.0** or higher
- **ASP.NET Core 8.0+** (for AspNetExtensions package)
- **Entity Framework Core 8.0+** (for `ToPagedResultAsync()` in Pagination package)

## License

MIT License - See [LICENSE](LICENSE) for details.

**Built with care for clean, consistent API responses.**
