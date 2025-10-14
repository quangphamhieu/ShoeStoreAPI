using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Store;
using ShoeStore.Application.Interfaces.Services;

namespace ShoeStore.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<StoreDto>>> GetAllStores()
        {
            try
            {
                var stores = await _storeService.GetAllAsync();
                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStoreById(int id)
        {
            try
            {
                var store = await _storeService.GetByIdAsync(id);
                return store == null ? NotFound() : Ok(store);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<StoreDto>> CreateStore(CreateStoreDto dto)
        {
            try
            {
                var store = await _storeService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetStoreById), new { id = store.Id }, store);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StoreDto>> UpdateStore(int id, UpdateStoreDto dto)
        {
            try
            {
                var store = await _storeService.UpdateAsync(id, dto);
                return store == null ? NotFound() : Ok(store);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            try
            {
                var deleted = await _storeService.DeleteAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
