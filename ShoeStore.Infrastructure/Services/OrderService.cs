using Microsoft.EntityFrameworkCore;
using ShoeStore.Application.Dtos.Order;
using ShoeStore.Application.Interfaces.Services;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var productIds = dto.OrderDetails.Select(x => x.ProductId).ToList();

            // Lấy product từ DB
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Kiểm tra sản phẩm tồn tại
            if (products.Count != productIds.Count)
                throw new Exception("Một hoặc nhiều sản phẩm không tồn tại");

            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0;

            foreach (var item in dto.OrderDetails)
            {
                if (item.Quantity <= 0)
                    throw new Exception("Số lượng phải lớn hơn 0");

                var product = products[item.ProductId];
                var lineTotal = product.CostPrice * item.Quantity;

                orderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.CostPrice, // tự set giá từ DB
                });

                totalAmount += lineTotal;
            }

            var order = new Order
            {
                OrderNumber = $"OD-{DateTime.UtcNow.Ticks}",
                CustomerId = userId,
                StoreId = dto.StoreId,
                OrderType = dto.OrderType,
                PaymentMethod = dto.PaymentMethod,
                StatusId = 4,
                TotalAmount = totalAmount, // tự tính
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                OrderDetails = orderDetails
            };

            await _context.Orders.AddAsync(order);
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
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(o => new OrderResponseDto
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
                    UnitPrice = d.UnitPrice,
                }).ToList()
            });
        }

        public async Task<bool> UpdateOrderAsync(long id, OrderUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
                return false;

            var oldStatus = order.StatusId;
            var newStatus = dto.StatusId;

            order.StatusId = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // 🔻 Nếu đơn chuyển sang "Xác nhận" (CONFIRMED = 5)
            if (newStatus == 5 && oldStatus != 5)
            {
                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.StoreId == order.StoreId)
                    .ToDictionaryAsync(sp => sp.ProductId);

                foreach (var detail in order.OrderDetails)
                {
                    if (!storeProducts.ContainsKey(detail.ProductId))
                        throw new Exception($"Sản phẩm ID {detail.ProductId} không có trong cửa hàng này");

                    var sp = storeProducts[detail.ProductId];
                    if (sp.Quantity < detail.Quantity)
                        throw new Exception($"Sản phẩm '{detail.ProductId}' không đủ hàng (còn {sp.Quantity})");

                    sp.Quantity -= detail.Quantity;
                }
            }

            // 🔁 Nếu đơn chuyển sang "Đã hủy" (CANCELLED = 6)
            else if (newStatus == 6 && oldStatus == 5)
            {
                // chỉ hoàn kho nếu đơn từng được xác nhận
                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.StoreId == order.StoreId)
                    .ToDictionaryAsync(sp => sp.ProductId);

                foreach (var detail in order.OrderDetails)
                {
                    if (storeProducts.ContainsKey(detail.ProductId))
                    {
                        storeProducts[detail.ProductId].Quantity += detail.Quantity;
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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
        public async Task<IEnumerable<OrderResponseDto>> GetOrderByUserAsync(long userId)
        {
            var orders = await _context.Orders.
                Include(o => o.Customer)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails).ThenInclude(p => p.Product)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();
            return orders.Select(o => new OrderResponseDto
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
                    UnitPrice = d.UnitPrice,
                }).ToList()
            });
        }
    }
}
