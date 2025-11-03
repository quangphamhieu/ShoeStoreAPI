using ShoeStore.Application.Dtos.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto, long userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(long id);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
        Task<bool> UpdateOrderAsync(long id, OrderUpdateDto dto);
        Task<bool> DeleteOrderAsync(long id);
    }
}
