using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Product;
using ShoeStore.Application.Interfaces.Services;

namespace ShoeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDto>>> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return product == null ? NotFound() : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
        {
            try
            {
                var created = await _productService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductDto dto)
        {
            try
            {
                var updated = await _productService.UpdateAsync(id, dto);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _productService.DeleteAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult> Search([FromBody] SearchProductDto searchDto)
        {
            try
            {
                var result = await _productService.SearchAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("suggest")]
        public async Task<ActionResult> Suggest([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return Ok(Enumerable.Empty<string>());

                var suggestions = await _productService.SuggestAsync(keyword);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{productId}/store-quantity")]
        public async Task<ActionResult> CreateStoreQuantity(int productId, [FromBody] StoreQuantityDto dto)
        {
            try
            {
                if (dto == null || dto.StoreId <= 0)
                    return BadRequest(new { message = "Invalid StoreQuantityDto" });

                var created = await _productService.CreateStoreQuantityAsync(dto, productId);
                return created == null
                    ? Conflict(new { message = "Relation already exists." })
                    : Ok(created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{productId}/store-quantity")]
        public async Task<ActionResult> UpdateStoreQuantity(int productId, [FromBody] StoreQuantityDto dto)
        {
            try
            {
                if (dto == null || dto.StoreId <= 0)
                    return BadRequest(new { message = "Invalid StoreQuantityDto" });

                var updated = await _productService.UpdateStoreQuantityAsync(dto, productId);
                return updated == null
                    ? NotFound(new { message = $"No relation found for ProductId {productId} and StoreId {dto.StoreId}." })
                    : Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
