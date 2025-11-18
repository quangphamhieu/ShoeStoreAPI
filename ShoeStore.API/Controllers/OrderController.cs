using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Order;
using ShoeStore.Application.Interfaces.Services;
using System.Threading.Tasks;

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
            var userId = long.Parse(User.FindFirst("userId")!.Value);
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

        [HttpGet("myOrder")]
        public async Task<IActionResult> GetOrderByUser()
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            var result = await _service.GetOrderByUserAsync(userId);
            return Ok(result);
        }

        [HttpPut("detail/{orderDetailId:long}")]
        public async Task<IActionResult> UpdateDetail(long orderDetailId, [FromBody] OrderDetailUpdateDto dto)
        {
            dto.OrderDetailId = orderDetailId;
            var success = await _service.UpdateOrderDetailAsync(dto);
            return success ? Ok() : NotFound();
        }

        [HttpDelete("detail/{orderDetailId:long}")]
        public async Task<IActionResult> DeleteDetail(long orderDetailId)
        {
            var success = await _service.DeleteOrderDetailAsync(orderDetailId);
            return success ? Ok() : NotFound();
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] OrderStatusUpdateDto dto)
        {
            var success = await _service.UpdateOrderStatusAsync(dto);
            return success ? Ok() : NotFound();
        }
    }
}
