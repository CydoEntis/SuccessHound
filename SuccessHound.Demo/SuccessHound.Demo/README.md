# SuccessHound Demo Application

A complete, working demonstration of **SuccessHound v1.0.0** with all features showcased.

## What This Demo Shows

This minimal API application demonstrates:

✅ **DI-First Configuration** - No static state, all services registered with dependency injection
✅ **All Extension Methods** - `.Ok()`, `.Created()`, `.Updated()`, `.Deleted()`, `.WithMeta()`, `.Custom()`
✅ **EF Core Pagination** - Automatic pagination with `ToPagedResultAsync()`
✅ **In-Memory Pagination** - Paginate collections with `ToPagedResult()`
✅ **Custom Formatters** - Swap between default and custom response formats
✅ **Null Handling** - Safe handling of null data
✅ **Complex Metadata** - Attach rich metadata to responses
✅ **Full CRUD Operations** - Complete Create, Read, Update, Delete examples

---

## Quick Start

### 1. Run the Application

```bash
cd SuccessHound.Demo/SuccessHound.Demo
dotnet run
```

The API will start at `https://localhost:5001` (or `http://localhost:5000`)

### 2. Open Swagger UI

Navigate to: `https://localhost:5001/swagger`

### 3. Try the Endpoints

#### Get Root Instructions
```bash
curl https://localhost:5001/
```

#### Get a Single User
```bash
curl https://localhost:5001/api/users/1
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "isActive": true,
    "createdAt": "2024-11-21T10:30:00.000Z"
  },
  "meta": null,
  "timestamp": "2025-12-21T18:00:00.000Z"
}
```

#### Get Paginated Users
```bash
curl "https://localhost:5001/api/users?page=1&pageSize=3"
```

**Response:**
```json
{
  "success": true,
  "data": [
    { "id": 9, "name": "Iris West", ... },
    { "id": 10, "name": "Jack Ryan", ... },
    { "id": 7, "name": "Grace Hopper", ... }
  ],
  "meta": {
    "pagination": {
      "page": 1,
      "pageSize": 3,
      "totalCount": 8,
      "totalPages": 3,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
  },
  "timestamp": "2025-12-21T18:00:00.000Z"
}
```

#### Get User with Custom Metadata
```bash
curl https://localhost:5001/api/users/1/details
```

**Response:**
```json
{
  "success": true,
  "data": { ... },
  "meta": {
    "requestedAt": "2025-12-21T18:00:00.000Z",
    "requestedBy": "Demo User",
    "version": "1.0",
    "source": "SuccessHound Demo API"
  },
  "timestamp": "2025-12-21T18:00:00.000Z"
}
```

#### Create a New User
```bash
curl -X POST https://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New User",
    "email": "newuser@example.com",
    "isActive": true
  }'
```

**Response:** `201 Created` with `Location: /api/users/{id}` header

```json
{
  "success": true,
  "data": {
    "id": 11,
    "name": "New User",
    "email": "newuser@example.com",
    "isActive": true,
    "createdAt": "2025-12-21T18:00:00.000Z"
  },
  "meta": null,
  "timestamp": "2025-12-21T18:00:00.000Z"
}
```

---

## Code Walkthrough

### 1. DI Configuration (Program.cs)

```csharp
// ✅ Register SuccessHound with dependency injection
builder.Services.AddSuccessHound(options =>
{
    // Use the default formatter
    options.UseFormatter<DefaultSuccessFormatter>();

    // Enable pagination
    options.UsePagination();
});

// ✅ Add middleware (optional)
app.UseSuccessHound();
```

**What happens:**
- `ISuccessResponseFormatter` is registered with the DI container
- `IPaginationMetadataFactory` is registered for pagination
- No static state - everything is injected per-request

---

### 2. Basic Endpoint (Program.cs)

```csharp
app.MapGet("/api/users/{id}", async (int id, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    // ✅ Wrap in SuccessHound envelope
    return user.Ok(context);
    //              ▲
    //              │
    //    ASP.NET injects HttpContext automatically
});
```

**How it works:**
1. ASP.NET creates `HttpContext` with `RequestServices` (DI container)
2. You call `.Ok(context)`
3. `.Ok()` gets formatter from `context.RequestServices`
4. Formatter wraps your data in the success envelope

---

### 3. Pagination Endpoint (Program.cs)

```csharp
app.MapGet("/api/users", async (DemoDbContext db, HttpContext context, int page = 1, int pageSize = 5) =>
{
    // ✅ Automatic pagination with metadata
    return await db.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .ToPagedResultAsync(page, pageSize, context);
        //                                   ▲
        //                                   │
        //                          Gets formatter & pagination factory from DI
});
```

**What happens:**
1. Query is executed with `Skip()` and `Take()`
2. Total count is calculated
3. Pagination factory creates metadata
4. Both are wrapped using the formatter
5. Returns `IResult` with paginated data + metadata

---

### 4. Custom Formatter (Formatters/CustomJsonApiFormatter.cs)

```csharp
public sealed class CustomJsonApiFormatter : ISuccessResponseFormatter
{
    public object Format(object? data, object? meta = null)
    {
        return new
        {
            jsonapi = new { version = "1.0" },
            data,
            meta,
            links = new { self = "/api/current-endpoint" }
        };
    }
}
```

**To use it:**

In `Program.cs`, change:
```csharp
builder.Services.AddSuccessHound(options =>
{
    // options.UseFormatter<DefaultSuccessFormatter>(); // ❌ Comment out
    options.UseFormatter<CustomJsonApiFormatter>();     // ✅ Use custom
    options.UsePagination();
});
```

Now all responses will use the JSON:API format!

---

## All Endpoints

| Method | Endpoint | Description | Feature |
|--------|----------|-------------|---------|
| `GET` | `/` | Root with instructions | Basic `.Ok()` |
| `GET` | `/api/users/{id}` | Get user by ID | `.Ok()` |
| `GET` | `/api/users` | List users (paginated) | `.ToPagedResultAsync()` |
| `POST` | `/api/users` | Create user | `.Created()` |
| `PUT` | `/api/users/{id}` | Update user | `.Updated()` |
| `DELETE` | `/api/users/{id}` | Delete user | `.Deleted()` |
| `GET` | `/api/users/{id}/details` | User with metadata | `.WithMeta()` |
| `POST` | `/api/users/{id}/activate` | Activate user | `.Custom(202)` |
| `GET` | `/api/users/active` | Active users | Collection `.Ok()` |
| `GET` | `/api/users/{id}/optional` | User (may be null) | Null handling |
| `GET` | `/api/products` | List products (paginated) | `.ToPagedResultAsync()` |
| `GET` | `/api/stats` | Stats (in-memory pagination) | `.ToPagedResult()` |
| `GET` | `/api/report/summary` | Summary report | Complex metadata |
| `GET` | `/api/demo/custom-format` | Demo custom formatter | Custom formatter |

---

## Switching Formatters

### Using Default Formatter

```csharp
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<DefaultSuccessFormatter>();
    options.UsePagination();
});
```

**Response Shape:**
```json
{
  "success": true,
  "data": { ... },
  "meta": { ... },
  "timestamp": "2025-12-21T18:00:00.000Z"
}
```

---

### Using Custom JSON:API Formatter

```csharp
builder.Services.AddSuccessHound(options =>
{
    options.UseFormatter<CustomJsonApiFormatter>();
    options.UsePagination();
});
```

**Response Shape:**
```json
{
  "jsonapi": { "version": "1.0" },
  "data": { ... },
  "meta": { ... },
  "links": { "self": "/api/current-endpoint" }
}
```

**No endpoint code changes required!** Just swap the formatter registration.

---

## Key Concepts Demonstrated

### 1. No Static State

**Before (v0.1.0):**
```csharp
SuccessHound.Configure(config => config.UseDefaultApiResponse());
// ❌ Sets global static field

return user.Ok();
// ❌ Reaches into global static state
```

**After (v1.0.0):**
```csharp
builder.Services.AddSuccessHound(options => options.UseFormatter<DefaultSuccessFormatter>());
// ✅ Registers with DI container

return user.Ok(context);
// ✅ Gets formatter from DI via context
```

---

### 2. Testability

```csharp
// You can create isolated test environments
var services = new ServiceCollection();
services.AddSingleton<ISuccessResponseFormatter, MyTestFormatter>();
var provider = services.BuildServiceProvider();

var context = new DefaultHttpContext { RequestServices = provider };

// Test uses YOUR formatter, not a global one
var result = user.Ok(context);
```

---

### 3. Explicit Dependencies

```csharp
// Before: Hidden dependency
return user.Ok();  // Where does formatter come from? 🤷‍♂️

// After: Explicit dependency
return user.Ok(context);  // Ah, needs HttpContext! ✅
```

ASP.NET automatically injects `HttpContext`, so zero extra work.

---

## Understanding the Flow

```
1. Request: GET /api/users/1
   ↓
2. ASP.NET creates HttpContext
   ↓
3. Populates context.RequestServices = DI Container
   ↓
4. Endpoint executes: (int id, DemoDbContext db, HttpContext context)
   ↓
5. Finds user in database
   ↓
6. Calls: user.Ok(context)
   ↓
7. Ok() method:
   - Gets formatter: context.RequestServices.GetService<ISuccessResponseFormatter>()
   - Calls: formatter.Format(user, meta: null)
   ↓
8. Returns formatted response:
   {
     "success": true,
     "data": { user object },
     "meta": null,
     "timestamp": "..."
   }
```

---

## Learning Path

1. **Run the app** - See it working
2. **Try all endpoints** - Use Swagger UI
3. **Read Program.cs** - Understand configuration
4. **Switch formatters** - See how easy it is
5. **Read REFACTOR_GUIDE.md** - Deep dive into "why"
6. **Modify endpoints** - Add your own
7. **Write tests** - See how testable it is

---

## What You'll Learn

- ✅ How DI works in ASP.NET Core
- ✅ Why static state is problematic
- ✅ How to configure services at startup
- ✅ How to inject dependencies per-request
- ✅ How to write testable code
- ✅ How to build composable libraries
- ✅ Industry best practices for .NET APIs

---

## Next Steps

1. **Explore the code** - Every endpoint is documented with comments
2. **Make changes** - Try adding your own endpoints
3. **Test it** - Write unit tests using the DI approach
4. **Read the guide** - Check out `REFACTOR_GUIDE.md` in the parent directory
5. **Build your own** - Apply these patterns to your projects

---

## Common Questions

### Why pass `HttpContext` to every method?

It makes dependencies **visible** instead of **hidden**. ASP.NET automatically injects it, so it's zero effort:

```csharp
app.MapGet("/users/{id}", (int id, HttpContext context) =>
{
    //                                 ▲
    //                                 │
    //                    Automatically injected by ASP.NET
});
```

### Can I avoid passing `HttpContext`?

Yes, but it adds complexity. The `HttpContext` approach is simpler and more standard. See the `REFACTOR_GUIDE.md` for alternatives.

### How do I test endpoints?

```csharp
var services = new ServiceCollection();
services.AddSingleton<ISuccessResponseFormatter, MyTestFormatter>();
var provider = services.BuildServiceProvider();

var context = new DefaultHttpContext { RequestServices = provider };

var result = user.Ok(context);
// Uses YOUR test formatter!
```

---

## Technology Stack

- **.NET 8.0**
- **ASP.NET Core Minimal APIs**
- **Entity Framework Core (In-Memory)**
- **SuccessHound v1.0.0**
- **Swagger/OpenAPI**

---

## License

MIT License - Same as SuccessHound

---

**Built to demonstrate modern .NET API best practices with SuccessHound.**
