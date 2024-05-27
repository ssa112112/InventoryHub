using AutoMapper;
using InventoryHub.DTOs;
using InventoryHub.Models;

namespace InventoryHub.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>().ReverseMap();
        }
    }
}
