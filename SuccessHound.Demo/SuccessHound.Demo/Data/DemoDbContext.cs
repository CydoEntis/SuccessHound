using Microsoft.EntityFrameworkCore;
using SuccessHound.Demo.Models;

namespace SuccessHound.Demo.Data;

public class DemoDbContext : DbContext
{
    public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new User { Id = 2, Name = "Bob Smith", Email = "bob@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new User { Id = 3, Name = "Charlie Brown", Email = "charlie@example.com", IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new User { Id = 4, Name = "Diana Prince", Email = "diana@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new User { Id = 5, Name = "Eve Martinez", Email = "eve@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new User { Id = 6, Name = "Frank Castle", Email = "frank@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new User { Id = 7, Name = "Grace Hopper", Email = "grace@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new User { Id = 8, Name = "Henry Ford", Email = "henry@example.com", IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new User { Id = 9, Name = "Iris West", Email = "iris@example.com", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new User { Id = 10, Name = "Jack Ryan", Email = "jack@example.com", IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 1299.99m, Stock = 15, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new Product { Id = 2, Name = "Mouse", Description = "Wireless mouse", Price = 29.99m, Stock = 50, CreatedAt = DateTime.UtcNow.AddDays(-55) },
            new Product { Id = 3, Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99m, Stock = 30, CreatedAt = DateTime.UtcNow.AddDays(-50) },
            new Product { Id = 4, Name = "Monitor", Description = "27-inch 4K monitor", Price = 449.99m, Stock = 20, CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new Product { Id = 5, Name = "Webcam", Description = "HD webcam", Price = 79.99m, Stock = 25, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new Product { Id = 6, Name = "Headphones", Description = "Noise-cancelling headphones", Price = 199.99m, Stock = 40, CreatedAt = DateTime.UtcNow.AddDays(-35) },
            new Product { Id = 7, Name = "USB Hub", Description = "7-port USB hub", Price = 24.99m, Stock = 60, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new Product { Id = 8, Name = "Desk Lamp", Description = "LED desk lamp", Price = 39.99m, Stock = 35, CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new Product { Id = 9, Name = "Phone Stand", Description = "Adjustable phone stand", Price = 19.99m, Stock = 100, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Product { Id = 10, Name = "Cable Organizer", Description = "Cable management system", Price = 14.99m, Stock = 75, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new Product { Id = 11, Name = "External SSD", Description = "1TB external SSD", Price = 129.99m, Stock = 18, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new Product { Id = 12, Name = "Laptop Stand", Description = "Ergonomic laptop stand", Price = 49.99m, Stock = 22, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        );
    }
}
