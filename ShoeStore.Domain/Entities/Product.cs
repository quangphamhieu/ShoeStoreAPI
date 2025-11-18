namespace ShoeStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string? SKU { get; set; }
        public string Name { get; set; } = null!;
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public decimal CostPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public Brand? Brand { get; set; }
        public Supplier? Supplier { get; set; }
        public Status Status { get; set; } = null!;

        public ICollection<StoreProduct>? StoreProducts { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
