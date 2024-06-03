using AutoMapper;
using InventoryHub.DTOs;
using InventoryHub.Models;
using InventoryHub.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateAsync([FromBody] CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            await _productRepository.AddAsync(product);
            var productDto = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = product.Id }, productDto);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateAsync(Guid id, [FromBody] PatchProductDto patchProductDto)
        {
            var product = await _productRepository.PatchProductAsync(id, patchProductDto.Name);
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ProductDto>> DeleteAsync(Guid id)
        {
            var product = await _productRepository.DeleteAsync(id);
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }


        [HttpPatch("{id}/adjust")]
        public async Task<ActionResult<ProductDto>> AdjustQuantityAsync(Guid id, [FromBody] int adjustment)
        {
            var (product, isUpdated) = await _productRepository.AdjustQuantityAsync(id, adjustment);
            var productDto = _mapper.Map<ProductDto>(product);

            if (isUpdated == false)
            {
                var errorResponse = new
                {
                    error = "Insufficient stock. The operation would result in negative quantity.",
                    product = productDto
                };

                return Conflict(errorResponse);
            }

            return Ok(productDto);
        }
    }
}
