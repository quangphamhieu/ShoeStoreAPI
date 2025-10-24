using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Receipt;
using ShoeStore.Application.Interfaces.Services;

namespace ShoeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiptDto>>> GetAll()
        {
            try
            {
                var receipts = await _receiptService.GetAllAsync();
                return Ok(receipts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptDto>> GetById(long id)
        {
            try
            {
                var receipt = await _receiptService.GetByIdAsync(id);
                return receipt == null ? NotFound() : Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ReceiptDto>> Create(CreateReceiptDto dto)
        {
            try
            {
                var created = await _receiptService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/info")]
        public async Task<ActionResult<ReceiptDto>> UpdateInfo(long id, UpdateReceiptDto dto)
        {
            try
            {
                var updated = await _receiptService.UpdateAsync(id, dto);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}/receive")]
        public async Task<ActionResult<ReceiptDto>> UpdateReceived(long id, UpdateReceiptReceivedDto dto)
        {
            try
            {
                var updated = await _receiptService.UpdateReceivedAsync(id, dto);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ✅ Thêm endpoint xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var deleted = await _receiptService.DeleteAsync(id);
                if (!deleted) return NotFound();

                return NoContent(); // HTTP 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
