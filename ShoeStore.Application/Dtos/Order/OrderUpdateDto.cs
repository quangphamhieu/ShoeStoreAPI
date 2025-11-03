using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Order
{
    public class OrderUpdateDto
    {
        public int StatusId { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
