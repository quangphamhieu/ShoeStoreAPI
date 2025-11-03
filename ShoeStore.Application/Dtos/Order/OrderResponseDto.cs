using ShoeStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Order
{
    public class OrderResponseDto
    {
        public long Id { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailResponseDto>? OrderDetails { get; set; }
    }

    public class OrderDetailResponseDto
    {
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
