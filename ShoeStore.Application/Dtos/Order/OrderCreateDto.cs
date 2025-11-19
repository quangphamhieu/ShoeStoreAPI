using ShoeStore.Domain.Entities;

namespace ShoeStore.Application.Dtos.Order
{
    public class OrderCreateDto
    {
        public long CustomerId { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Offline orders require StoreId (store of the staff creating the order).
        /// Online orders ignore this value and will automatically use the warehouse store (Id = 1).
        /// </summary>
        public int? StoreId { get; set; }

        public List<OrderDetailCreateDto> Details { get; set; } = new();
    }
}