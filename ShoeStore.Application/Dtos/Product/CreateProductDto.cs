using Microsoft.AspNetCore.Http;

namespace ShoeStore.Application.Dtos.Product
{
    public class CreateProductDto
    {
        public string? Name { get; set; }
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int StatusId { get; set; }
    }
}
