using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public long CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
