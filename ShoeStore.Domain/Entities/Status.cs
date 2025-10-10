namespace ShoeStore.Domain.Entities
{
    public class Status
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation (các bảng có thể lọc theo status)
        public ICollection<Store>? Stores { get; set; }
        public ICollection<User>? Users { get; set; }
        public ICollection<Product>? Products { get; set; }
        public ICollection<Brand>? Brands { get; set; }
        public ICollection<Supplier>? Suppliers { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public ICollection<Receipt>? Receipts { get; set; }
        public ICollection<Promotion>? Promotions { get; set; }
    }
}
