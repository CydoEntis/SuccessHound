using Microsoft.EntityFrameworkCore;
using SuccessHound.AspNetExtensions;
using SuccessHound.Demo.Data;
using SuccessHound.Demo.Formatters;
using SuccessHound.Demo.Models;
using SuccessHound.Defaults;
using SuccessHound.Extensions;
using SuccessHound.Pagination;
using SuccessHound.Pagination.Extensions;
using SuccessHound.Pagination.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseInMemoryDatabase("DemoDb"));

// Configure SuccessHound with DI
builder.Services.AddSuccessHound(options =>
{
    // Use the default formatter
    options.UseFormatter<DefaultSuccessFormatter>();

    // Or use a custom formatter (uncomment to try):
    // options.UseFormatter<CustomJsonApiFormatter>();

    // Enable pagination
    options.UsePagination();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Add SuccessHound middleware (optional, currently pass-through)
app.UseSuccessHound();


var api = app.MapGroup("/api");

// ────────────────────────────────────────────────────────────────────────────────
// 1. Basic CRUD with .Ok() extension
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/users/{id}", async (int id, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    return user.Ok(context);
})
.WithName("GetUser")
.WithTags("Users")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 2. .Created() with Location header
// ────────────────────────────────────────────────────────────────────────────────

api.MapPost("/users", async (User user, DemoDbContext db, HttpContext context) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return user.Created($"/api/users/{user.Id}", context);
})
.WithName("CreateUser")
.WithTags("Users")
.Produces(201);

// ────────────────────────────────────────────────────────────────────────────────
// 3. .Updated() for PUT/PATCH operations
// ────────────────────────────────────────────────────────────────────────────────

api.MapPut("/users/{id}", async (int id, User updatedUser, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    user.IsActive = updatedUser.IsActive;

    await db.SaveChangesAsync();

    return user.Updated(context);
})
.WithName("UpdateUser")
.WithTags("Users")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 4. .Deleted() for DELETE operations (204 No Content)
// ────────────────────────────────────────────────────────────────────────────────

api.MapDelete("/users/{id}", async (int id, DemoDbContext db) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    db.Users.Remove(user);
    await db.SaveChangesAsync();

    return SuccessHound.AspNetExtensions.SuccessHoundResultsExtensions.Deleted();
})
.WithName("DeleteUser")
.WithTags("Users")
.Produces(204);

// ────────────────────────────────────────────────────────────────────────────────
// 5. .WithMeta() for custom metadata
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/users/{id}/details", async (int id, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    var meta = new
    {
        requestedAt = DateTime.UtcNow,
        requestedBy = "Demo User",
        version = "1.0",
        source = "SuccessHound Demo API"
    };

    return user.WithMeta(meta, context);
})
.WithName("GetUserDetails")
.WithTags("Users")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 6. .Custom() for custom HTTP status codes
// ────────────────────────────────────────────────────────────────────────────────

api.MapPost("/users/{id}/activate", async (int id, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    if (user is null)
        return Results.NotFound(new { error = "User not found" });

    user.IsActive = true;
    await db.SaveChangesAsync();

    return user.Custom(202, context);
})
.WithName("ActivateUser")
.WithTags("Users")
.Produces(202);

// ────────────────────────────────────────────────────────────────────────────────
// 7. Pagination with EF Core (.ToPagedResultAsync)
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/users", async (DemoDbContext db, HttpContext context, int page = 1, int pageSize = 5) =>
{
    return await db.Users
        .Where(u => u.IsActive)
        .OrderBy(u => u.CreatedAt)
        .ToPagedResultAsync(page, pageSize, context);
})
.WithName("ListUsers")
.WithTags("Users")
.Produces(200);

api.MapGet("/products", async (DemoDbContext db, HttpContext context, int page = 1, int pageSize = 10) =>
{
    return await db.Products
        .Where(p => p.Stock > 0)
        .OrderByDescending(p => p.CreatedAt)
        .ToPagedResultAsync(page, pageSize, context);
})
.WithName("ListProducts")
.WithTags("Products")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 8. In-Memory Pagination (.ToPagedResult)
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/stats", (HttpContext context, int page = 1, int pageSize = 3) =>
{
    var stats = new[]
    {
        new { Category = "Electronics", Count = 245, Revenue = 125000.50m },
        new { Category = "Books", Count = 892, Revenue = 45000.25m },
        new { Category = "Clothing", Count = 456, Revenue = 78000.00m },
        new { Category = "Home & Garden", Count = 123, Revenue = 34000.75m },
        new { Category = "Sports", Count = 678, Revenue = 89000.50m },
        new { Category = "Toys", Count = 234, Revenue = 23000.00m }
    };

    return stats.ToPagedResult(page, pageSize, context);
})
.WithName("GetStats")
.WithTags("Analytics")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 9. Handling null data 
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/users/{id}/optional", async (int id, DemoDbContext db, HttpContext context) =>
{
    var user = await db.Users.FindAsync(id);

    // SuccessHound handles null data 
    return user.Ok(context);
})
.WithName("GetUserOptional")
.WithTags("Users")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 10. Working with collections
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/users/active", async (DemoDbContext db, HttpContext context) =>
{
    var activeUsers = await db.Users
        .Where(u => u.IsActive)
        .ToListAsync();

    return activeUsers.Ok(context);
})
.WithName("GetActiveUsers")
.WithTags("Users")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 11. Complex metadata example
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/report/summary", async (DemoDbContext db, HttpContext context) =>
{
    var totalUsers = await db.Users.CountAsync();
    var activeUsers = await db.Users.CountAsync(u => u.IsActive);
    var totalProducts = await db.Products.CountAsync();
    var totalValue = await db.Products.SumAsync(p => p.Price * p.Stock);

    var report = new
    {
        TotalUsers = totalUsers,
        ActiveUsers = activeUsers,
        TotalProducts = totalProducts,
        InventoryValue = totalValue
    };

    var meta = new
    {
        generatedAt = DateTime.UtcNow,
        generatedBy = "System",
        reportType = "Summary",
        version = "2.0",
        filters = new { IncludeInactive = false },
        expiresAt = DateTime.UtcNow.AddHours(1)
    };

    return report.WithMeta(meta, context);
})
.WithName("GetSummaryReport")
.WithTags("Analytics")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 12. Demonstrating Custom Formatter
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/demo/custom-format", (HttpContext context) =>
{
    var data = new
    {
        message = "This response uses the configured formatter",
        tip = "Change the formatter in Program.cs to see different response shapes!"
    };

    return data.Ok(context);
})
.WithName("DemoCustomFormatter")
.WithTags("Demo")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// 13. Strongly-Typed Metadata Example (v2.0.0 Feature)
// ────────────────────────────────────────────────────────────────────────────────

api.MapGet("/demo/strongly-typed-meta", async (DemoDbContext db, HttpContext context) =>
{
    var totalUsers = await db.Users.CountAsync();
    var activeUsers = await db.Users.CountAsync(u => u.IsActive);
    
    var data = new
    {
        TotalUsers = totalUsers,
        ActiveUsers = activeUsers,
        Message = "This demonstrates strongly-typed metadata with PaginationMeta"
    };

    // Get pagination factory to create strongly-typed metadata
    var factory = context.RequestServices.GetRequiredService<SuccessHound.Pagination.Abstractions.IPaginationMetadataFactory>();
    var paginationMeta = factory.CreateMetadata(page: 1, pageSize: 10, totalCount: totalUsers);

    // Use strongly-typed WithMeta<TData, TMeta> overload
    return data.WithMeta(paginationMeta, context);
})
.WithName("DemoStronglyTypedMeta")
.WithTags("Demo")
.Produces(200);

// ────────────────────────────────────────────────────────────────────────────────
// Root endpoint with instructions
// ────────────────────────────────────────────────────────────────────────────────

app.MapGet("/", (HttpContext context) =>
{
    var instructions = new
    {
        message = "Welcome to SuccessHound Demo API!",
        documentation = "/swagger",
        examples = new
        {
            basicCrud = "/api/users/1",
            pagination = "/api/users?page=1&pageSize=5",
            withMetadata = "/api/users/1/details",
            customStatus = "POST /api/users/1/activate",
            inMemoryPagination = "/api/stats?page=1&pageSize=3",
            report = "/api/report/summary"
        },
        features = new[]
        {
            "✅ Dependency Injection (DI-first architecture)",
            "✅ Consistent response wrapping",
            "✅ Automatic pagination (EF Core & in-memory)",
            "✅ Custom metadata support",
            "✅ Custom formatters (swap in Program.cs)",
            "✅ Full CRUD operations",
            "✅ Null-safe data handling"
        }
    };

    return instructions.Ok(context);
})
.WithName("Root")
.Produces(200);

app.Run();
