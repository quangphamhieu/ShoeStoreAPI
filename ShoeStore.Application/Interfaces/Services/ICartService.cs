using ShoeStore.Application.Dtos.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartByUserIdAsync(long userId);
        Task<CartDto> AddToCartAsync(long userId, AddToCartRequest request);
        Task<CartDto> UpdateQuantityAsync(long userId, UpdateCartItemRequest request);
        Task<CartDto> RemoveItemAsync(long userId, long cartItemId);
        Task ClearCartAsync(long userId);
    }
}