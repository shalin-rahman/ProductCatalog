using ProductCatalog.Api.Domain;

namespace ProductCatalog.Api.Repositories;

/// <summary>
/// Defines the contract for Product data access operations.
/// Programming to an interface allows us to swap implementations (e.g., InMemory for tests).
/// </summary>
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}
