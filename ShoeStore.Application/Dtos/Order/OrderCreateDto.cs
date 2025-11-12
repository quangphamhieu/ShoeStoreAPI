using ShoeStore.Domain.Entities;

namespace ShoeStore.Application.Dtos.Order
{
    public class OrderCreateDto
    {
        public long CustomerId { get; set; }
        public int? StoreId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderDetailCreateDto> OrderDetails { get; set; } = new();
    }
}