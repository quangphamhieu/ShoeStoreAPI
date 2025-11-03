using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Order;
using ShoeStore.Application.Interfaces.Services;

namespace ShoeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value); // lấy từ claims trong thực tế
            var result = await _service.CreateOrderAsync(dto, userId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var result = await _service.GetOrderByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllOrdersAsync();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] OrderUpdateDto dto)
        {
            var success = await _service.UpdateOrderAsync(id, dto);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var success = await _service.DeleteOrderAsync(id);
            return success ? Ok() : NotFound();
        }
    }
}
