using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Order;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShoeStoreDbContext _context;

        public OrderService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto, long userId)
        {
            var order = new Order
            {
                OrderNumber = $"OD-{DateTime.UtcNow.Ticks}",
                CustomerId = userId,
                StoreId = dto.StoreId,
                OrderType = dto.OrderType,
                PaymentMethod = dto.PaymentMethod,
                TotalAmount = dto.TotalAmount,
                StatusId = 1, // ví dụ: 1 = Pending
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = dto.OrderDetails.Select(d => new OrderDetail
                {
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(long id)
        {
            var o = await _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.Status)
                .Include(x => x.OrderDetails)!.ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (o == null) return null;

            return new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.FullName,
                TotalAmount = o.TotalAmount,
                StatusName = o.Status.Name,
                CreatedAt = o.CreatedAt,
                OrderDetails = o.OrderDetails?.Select(d => new OrderDetailResponseDto
                {
                    ProductName = d.Product.Name,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Status)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer.FullName,
                TotalAmount = o.TotalAmount,
                StatusName = o.Status.Name,
                CreatedAt = o.CreatedAt
            });
        }

        public async Task<bool> UpdateOrderAsync(long id, OrderUpdateDto dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null) return false;

            order.StatusId = dto.StatusId;
            if (dto.TotalAmount.HasValue)
                order.TotalAmount = dto.TotalAmount.Value;

            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(long id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null) return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}