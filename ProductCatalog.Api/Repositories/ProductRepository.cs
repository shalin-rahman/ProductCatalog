using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Domain;

namespace ProductCatalog.Api.Repositories;

/// <summary>
/// Concrete implementation of IProductRepository using EF Core.
/// This is the ONLY place in the application that talks directly to the DbContext.
/// Scoped lifetime matches the DbContext lifetime (one instance per HTTP request).
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    /// <summary>
    /// DbContext is injected by the DI container. 
    /// Because both are registered as Scoped, they share the same lifetime per request.
    /// </summary>
    public ProductRepository(CatalogDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns all products as a list.
    /// AsNoTracking() improves performance for read-only queries — EF won't track changes.
    /// </summary>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Returns a single product by Id, or null if not found.
    /// Using FindAsync is optimized — it checks the in-memory cache first before hitting the DB.
    /// </summary>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Adds a new product to the database and returns the entity with the generated Id.
    /// </summary>
    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product; // EF Core populates the Id after SaveChangesAsync.
    }

    /// <summary>
    /// Updates an existing product.
    /// Returns false if the product does not exist (concurrency-safe check).
    /// </summary>
    public async Task<bool> UpdateAsync(Product product)
    {
        var exists = await _context.Products.AnyAsync(p => p.Id == product.Id);
        if (!exists) return false;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes a product by Id.
    /// Returns false if the product is not found.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
