using AutoMapper;
using ProductCatalog.Api.Domain;
using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Mapping;

/// <summary>
/// AutoMapper profile: defines mapping rules between Entities and DTOs.
/// AutoMapper uses this at startup to pre-compile the mapping functions.
/// Convention: if property names match, no explicit configuration is needed.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity → Read DTO
        CreateMap<Product, ProductDto>();

        // Create DTO → Entity (Id is excluded — DB generates it)
        CreateMap<ProductCreateDto, Product>();

        // Update DTO → Entity (Id will be set manually in the service layer)
        CreateMap<ProductUpdateDto, Product>();
    }
}
