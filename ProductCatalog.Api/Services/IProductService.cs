using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Services;

/// <summary>
/// Defines the business logic contract for Product operations.
/// Controllers depend on this interface, not the concrete class — enabling testability.
/// </summary>
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(ProductCreateDto dto);
    Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto);
    Task<bool> DeleteProductAsync(int id);
}
