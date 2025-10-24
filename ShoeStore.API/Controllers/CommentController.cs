using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Comment;
using ShoeStore.Application.Interfaces;

namespace ShoeStore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAll()
        {
            try
            {
                var comments = await _commentService.GetAllAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<CommentDto>> GetById(long id)
        {
            try
            {
                var comment = await _commentService.GetByIdAsync(id);
                return comment == null ? NotFound() : Ok(comment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("product/{productId:int}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetByProductId(int productId)
        {
            try
            {
                var comments = await _commentService.GetByProductIdAsync(productId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<CommentDto>> Create(CreateCommentDto dto)
        {
            try
            {
                var created = await _commentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult<CommentDto>> Update(long id, UpdateCommentDto dto)
        {
            try
            {
                var updated = await _commentService.UpdateAsync(id, dto);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<ActionResult> Delete(long id)
        {
            try
            {
                var deleted = await _commentService.DeleteAsync(id);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
