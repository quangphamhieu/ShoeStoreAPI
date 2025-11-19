using System;
using System.Collections.Generic;
using ShoeStore.Domain.Entities;

namespace ShoeStore.Application.Dtos.Order
{
    public class OrderResponseDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public long CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public long? CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public int? StoreId { get; set; }
        public OrderType OrderType { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int StatusId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderDetailResponseDto> Details { get; set; } = new();
    }

    public class OrderDetailResponseDto
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}