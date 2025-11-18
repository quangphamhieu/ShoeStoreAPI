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
        /// Online orders must keep this null because each detail will specify its store.
        /// </summary>
        public int? StoreId { get; set; }

        public List<OrderDetailCreateDto> Details { get; set; } = new();
    }
}