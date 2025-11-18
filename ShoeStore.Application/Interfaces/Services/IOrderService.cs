using ShoeStore.Application.Dtos.Order;
using ShoeStore.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto, long userId);
        Task<OrderResponseDto?> GetOrderByIdAsync(long id);
        Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderResponseDto>> GetOrderByUserAsync(long userId);

        Task<bool> UpdateOrderDetailAsync(OrderDetailUpdateDto dto);
        Task<bool> DeleteOrderDetailAsync(long orderDetailId);
        Task<bool> UpdateOrderStatusAsync(OrderStatusUpdateDto dto);
    }
}
