using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Domain;

namespace ProductCatalog.Api.Data;

/// <summary>
/// The EF Core session (Unit of Work) for the ProductCatalog database.
/// Inherits from DbContext to gain access to change tracking, querying, and saving.
/// </summary>
public class CatalogDbContext : DbContext
{
    /// <summary>
    /// Constructor receives options (e.g., the connection string) via Dependency Injection.
    /// This makes the context testable — in tests, you can pass an InMemory provider.
    /// </summary>
    /// <param name="options">Database provider options configured in Program.cs</param>
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Represents the Products table in the database.
    /// EF Core uses this DbSet to generate SQL queries and map results back to Product objects.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Fluent API configuration for the model.
    /// Called once by EF Core when building the internal model cache.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table name is pluralized by convention, but we set it explicitly for clarity.
        modelBuilder.Entity<Product>().ToTable("Products");

        // Price must be non-negative — enforced at DB level via a check constraint.
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
    }
}
