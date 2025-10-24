using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Receipt
{
    public class ReceiptDetailDto
    {
        public long Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public int QuantityOrdered { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
