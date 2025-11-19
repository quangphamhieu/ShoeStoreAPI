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
        private const int WarehouseStoreId = 1;

        public OrderService(ShoeStoreDbContext context)
        {
            _context = context;
        }

        // ================= CREATE ORDER =================
        public async Task<OrderResponseDto> CreateOrderAsync(OrderCreateDto dto, long userId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("Đơn hàng phải có ít nhất 1 sản phẩm.");

            var isOffline = dto.OrderType == OrderType.Offline;
            var isOnline = dto.OrderType == OrderType.Online;

            if (!isOffline && !isOnline)
                throw new Exception("Loại đơn hàng không hợp lệ.");

            if (isOffline && !dto.StoreId.HasValue)
                throw new Exception("Đơn offline phải chọn cửa hàng.");

            if (isOnline && dto.StoreId.HasValue && dto.StoreId.Value != WarehouseStoreId)
                throw new Exception("Đơn online chỉ có thể thao tác trên kho hàng.");

            var customerExists = await _context.Users.AnyAsync(u => u.Id == dto.CustomerId);
            if (!customerExists)
                throw new Exception("Customer không tồn tại.");

            var productIds = dto.Details.Select(i => i.ProductId).Distinct().ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            if (products.Count != productIds.Count)
                throw new Exception("Một hoặc nhiều sản phẩm không tồn tại.");

            var orderDetails = new List<OrderDetail>();
            decimal totalAmount = 0m;

            if (isOffline)
            {
                int storeId = dto.StoreId!.Value;

                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.StoreId == storeId && productIds.Contains(sp.ProductId))
                    .ToDictionaryAsync(sp => sp.ProductId);

                foreach (var item in dto.Details)
                {
                    if (item.Quantity <= 0) throw new Exception("Số lượng phải lớn hơn 0.");

                    if (!storeProducts.TryGetValue(item.ProductId, out var sp))
                        throw new Exception($"Sản phẩm '{products[item.ProductId].Name}' không có trong kho cửa hàng.");

                    if (sp.Quantity < item.Quantity)
                        throw new Exception($"Sản phẩm '{products[item.ProductId].Name}' chỉ còn {sp.Quantity} trong kho.");

                    sp.Quantity -= item.Quantity;

                    var od = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = sp.SalePrice
                    };

                    orderDetails.Add(od);
                    totalAmount += od.UnitPrice * od.Quantity;
                }

                var order = new Order
                {
                    OrderNumber = $"OD-{DateTime.UtcNow.Ticks}",
                    CustomerId = dto.CustomerId,
                    CreatedBy = userId,
                    StoreId = storeId,
                    OrderType = OrderType.Offline,
                    PaymentMethod = dto.PaymentMethod,
                    StatusId = 3,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    OrderDetails = orderDetails
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                return await GetOrderByIdAsync(order.Id) ?? throw new Exception("Tạo đơn thất bại.");
            }
            else
            {
                int storeId = WarehouseStoreId;

                var storeProducts = await _context.StoreProducts
                    .Where(sp => sp.StoreId == storeId && productIds.Contains(sp.ProductId))
                    .ToDictionaryAsync(sp => sp.ProductId);

                foreach (var item in dto.Details)
                {
                    if (item.Quantity <= 0) throw new Exception("Số lượng phải lớn hơn 0.");

                    if (!storeProducts.TryGetValue(item.ProductId, out var sp))
                        throw new Exception($"Sản phẩm '{products[item.ProductId].Name}' không có trong kho kho hàng.");

                    if (sp.Quantity < item.Quantity)
                        throw new Exception($"Sản phẩm '{products[item.ProductId].Name}' chỉ còn {sp.Quantity} trong kho kho hàng.");

                    sp.Quantity -= item.Quantity;

                    var od = new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = sp.SalePrice
                    };

                    orderDetails.Add(od);
                    totalAmount += od.UnitPrice * od.Quantity;
                }

                var order = new Order
                {
                    OrderNumber = $"OD-{DateTime.UtcNow.Ticks}",
                    CustomerId = dto.CustomerId,
                    CreatedBy = userId,
                    StoreId = storeId,
                    OrderType = OrderType.Online,
                    PaymentMethod = dto.PaymentMethod,
                    StatusId = 4,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    OrderDetails = orderDetails
                };

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                return await GetOrderByIdAsync(order.Id) ?? throw new Exception("Tạo đơn thất bại.");
            }
        }

        // ================= GET ORDER BY ID =================
        public async Task<OrderResponseDto?> GetOrderByIdAsync(long id)
        {
            var o = await _context.Orders
                .Include(x => x.Customer)
                .Include(x => x.Creator)
                .Include(x => x.Status)
                .Include(x => x.OrderDetails)!.ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (o == null) return null;

            return MapToResponseDto(o);
        }

        // ================= GET ALL ORDERS =================
        public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Creator)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(MapToResponseDto);
        }

        // ================= GET ORDERS BY USER =================
        public async Task<IEnumerable<OrderResponseDto>> GetOrderByUserAsync(long userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Creator)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            return orders.Select(MapToResponseDto);
        }

        // ================= UPDATE ORDER DETAIL (quantity only) =================
        // Only allowed when order is in PENDING_CONFIRMATION (statusId == 4)
        public async Task<bool> UpdateOrderDetailAsync(OrderDetailUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (dto.Quantity <= 0) throw new Exception("Số lượng phải lớn hơn 0.");

            var od = await _context.OrderDetails
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == dto.OrderDetailId);

            if (od == null) return false;

            var order = od.Order ?? throw new Exception("Order không tồn tại.");

            if (order.StatusId != 4)
                throw new Exception("Chỉ có thể chỉnh sửa số lượng khi đơn ở trạng thái chờ xác nhận (statusId == 4).");

            od.Quantity = dto.Quantity;

            await RecalculateOrderTotalAsync(order.Id);

            await _context.SaveChangesAsync();
            return true;
        }

        // ================= DELETE ORDER DETAIL =================
        // Only allowed when order is pending (statusId == 4)
        public async Task<bool> DeleteOrderDetailAsync(long orderDetailId)
        {
            var od = await _context.OrderDetails
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.Id == orderDetailId);

            if (od == null) return false;

            var order = od.Order ?? throw new Exception("Order không tồn tại.");

            if (order.StatusId != 4)
                throw new Exception("Chỉ có thể xóa item khi đơn ở trạng thái chờ xác nhận (statusId == 4).");

            _context.OrderDetails.Remove(od);

            await RecalculateOrderTotalAsync(order.Id);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(OrderStatusUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            using var transaction = await _context.Database.BeginTransactionAsync();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null) return false;
            if (order.OrderDetails == null || !order.OrderDetails.Any())
                throw new Exception("Order không có sản phẩm.");

            int oldStatus = order.StatusId;
            int newStatus = dto.StatusId;

            if (newStatus == 6 && (oldStatus == 3 || oldStatus == 5))
            {
                var storeId = order.StoreId ?? WarehouseStoreId;
                var storeProducts = await LoadStoreProductsAsync(order.OrderDetails, storeId);

                foreach (var detail in order.OrderDetails)
                {
                    if (storeProducts.TryGetValue(detail.ProductId, out var sp))
                    {
                        sp.Quantity += detail.Quantity;
                    }
                }
            }

            order.StatusId = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        // ================= HELPER: recalc total from order details =================
        private async Task RecalculateOrderTotalAsync(long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order không tồn tại.");

            order.TotalAmount = order.OrderDetails?.Sum(d => d.UnitPrice * d.Quantity) ?? 0m;
            order.UpdatedAt = DateTime.UtcNow;
        }

        private async Task<Dictionary<int, StoreProduct>> LoadStoreProductsAsync(IEnumerable<OrderDetail> details, int storeId)
        {
            var detailList = details?.ToList() ?? new List<OrderDetail>();
            if (!detailList.Any())
                return new Dictionary<int, StoreProduct>();

            var productIds = detailList.Select(d => d.ProductId).Distinct().ToList();
            return await _context.StoreProducts
                .Where(sp => sp.StoreId == storeId && productIds.Contains(sp.ProductId))
                .ToDictionaryAsync(sp => sp.ProductId);
        }

        // ================= MAPPING =================
        private OrderResponseDto MapToResponseDto(Order o)
        {
            return new OrderResponseDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.FullName ?? string.Empty,
                CreatedBy = o.CreatedBy,
                CreatorName = o.Creator?.FullName,
                StoreId = o.StoreId,
                OrderType = o.OrderType,
                PaymentMethod = o.PaymentMethod,
                StatusId = o.StatusId,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
                Details = o.OrderDetails?.Select(d => new OrderDetailResponseDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice
                }).ToList() ?? new List<OrderDetailResponseDto>()
            };
        }
    }
}
