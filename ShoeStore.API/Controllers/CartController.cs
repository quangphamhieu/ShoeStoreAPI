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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(long userId)
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return Ok(cart);
        }

        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddToCart(long userId, [FromBody] AddToCartRequest request)
        {
            var cart = await _cartService.AddToCartAsync(userId, request);
            return Ok(cart);
        }

        [HttpPut("{userId}/update")]
        public async Task<IActionResult> UpdateQuantity(long userId, [FromBody] UpdateCartItemRequest request)
        {
            var cart = await _cartService.UpdateQuantityAsync(userId, request);
            return Ok(cart);
        }

        [HttpDelete("{userId}/remove/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(long userId, long cartItemId)
        {
            var cart = await _cartService.RemoveItemAsync(userId, cartItemId);
            return Ok(cart);
        }

        [HttpDelete("{userId}/clear")]
        public async Task<IActionResult> ClearCart(long userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
