using Microsoft.AspNetCore.Mvc;
using ShoeStore.Application.Dtos.Cart;
using ShoeStore.Application.Interfaces.Services;

namespace ShoeStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("getCart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            var cart = await _cartService.AddToCartAsync(userId, request);
            return Ok(cart);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            var cart = await _cartService.UpdateQuantityAsync(userId, request);
            return Ok(cart);
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(long cartItemId)
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            var cart = await _cartService.RemoveItemAsync(userId, cartItemId);
            return Ok(cart);
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = long.Parse(User.FindFirst("userId")!.Value);
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}