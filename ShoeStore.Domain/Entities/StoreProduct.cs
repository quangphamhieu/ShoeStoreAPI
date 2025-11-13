namespace ShoeStore.Domain.Entities
{
    public class StoreProduct
    {
        public int StoreId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        // Navigation
        public Store Store { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
