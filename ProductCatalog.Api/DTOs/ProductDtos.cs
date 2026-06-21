using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.DTOs;

/// <summary>
/// DTO returned to the client for READ operations (GET).
/// Contains all fields including the server-generated Id.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
}

/// <summary>
/// DTO accepted from the client for CREATE operations (POST).
/// Does NOT include Id — the database generates it.
/// Validation attributes ensure the API rejects bad data before it hits business logic.
/// </summary>
public class ProductCreateDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}

/// <summary>
/// DTO accepted from the client for UPDATE operations (PUT).
/// Identical to CreateDto in this simple demo — in real apps,
/// you might allow partial updates or have different rules.
/// </summary>
public class ProductUpdateDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
