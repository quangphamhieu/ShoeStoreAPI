using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Cart;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;

namespace ShoeStore.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly ShoeStoreDbContext _context;

        public CartService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetCartByUserIdAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return new CartDto { UserId = userId };

            return new CartDto
            {
                Id = cart.Id,
                UserId = userId,
                Items = cart.CartItems?.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity
                }).ToList() ?? new()
            };
        }

        public async Task<CartDto> AddToCartAsync(long userId, AddToCartRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    StatusId = 1, // Giả định: 1 = Active
                    CreatedAt = DateTime.UtcNow,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.CartItems!.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(request.ProductId)
                    ?? throw new Exception("Sản phẩm không tồn tại");

                // ✅ Mặc định cửa hàng ID = 1 (có thể cần truyền từ request sau)
                int storeId = 1;

                // Lấy SalePrice từ StoreProduct
                var storeProduct = await _context.StoreProducts
                    .FirstOrDefaultAsync(sp => sp.ProductId == request.ProductId && sp.StoreId == storeId);

                if (storeProduct == null)
                    throw new Exception("Sản phẩm không có trong cửa hàng");

                cart.CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    UnitPrice = storeProduct.SalePrice
                });
            }

            await _context.SaveChangesAsync();
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDto> UpdateQuantityAsync(long userId, UpdateCartItemRequest request)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new Exception("Giỏ hàng không tồn tại");

            var item = cart.CartItems!.FirstOrDefault(i => i.Id == request.CartItemId)
                ?? throw new Exception("Sản phẩm trong giỏ không tồn tại");

            item.Quantity = request.Quantity;
            await _context.SaveChangesAsync();

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartDto> RemoveItemAsync(long userId, long cartItemId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new Exception("Giỏ hàng không tồn tại");

            var item = cart.CartItems!.FirstOrDefault(i => i.Id == cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return await GetCartByUserIdAsync(userId);
        }

        public async Task ClearCartAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            _context.CartItems.RemoveRange(cart.CartItems!);
            await _context.SaveChangesAsync();
        }
    }
}