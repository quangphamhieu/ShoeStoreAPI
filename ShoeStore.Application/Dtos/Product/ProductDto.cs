using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore.Application.Dtos.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string SKU { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public int? StoreId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
