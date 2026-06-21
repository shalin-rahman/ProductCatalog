using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// REST API controller for Product CRUD operations.
/// The controller's ONLY job is to:
///   1. Receive the HTTP request and validate model state.
///   2. Call the service layer.
///   3. Return the appropriate HTTP response.
/// No business logic lives here.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET api/products — Retrieve all products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// GET api/products/{id} — Retrieve a single product.
    /// Returns 404 if the product does not exist.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetProductByIdAsync(id);
        if (product is null)
            return NotFound(new { message = $"Product with Id {id} was not found." });

        return Ok(product);
    }

    /// <summary>
    /// POST api/products — Create a new product.
    /// Returns 201 Created with the Location header pointing to the new resource.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        // ModelState is automatically validated by [ApiController] — bad requests return 400.
        var created = await _service.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// PUT api/products/{id} — Update an existing product.
    /// Returns 204 No Content on success, 404 if not found.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
    {
        var updated = await _service.UpdateProductAsync(id, dto);
        if (!updated)
            return NotFound(new { message = $"Product with Id {id} was not found." });

        return NoContent();
    }

    /// <summary>
    /// DELETE api/products/{id} — Delete a product.
    /// Returns 204 No Content on success, 404 if not found.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteProductAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Product with Id {id} was not found." });

        return NoContent();
    }
}
