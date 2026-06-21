using AutoMapper;
using ProductCatalog.Api.Domain;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Repositories;

namespace ProductCatalog.Api.Services;

/// <summary>
/// Concrete implementation of IProductService.
/// Orchestrates calls to the repository and uses AutoMapper to translate between entities and DTOs.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(ProductCreateDto dto)
    {
        // Map DTO → Entity (no Id yet — DB will generate it)
        var product = _mapper.Map<Product>(dto);
        var created = await _repository.CreateAsync(product);
        // Map Entity → DTO (now has the generated Id)
        return _mapper.Map<ProductDto>(created);
    }

    public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        product.Id = id; // Attach the route Id to the entity before updating
        return await _repository.UpdateAsync(product);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }
}
