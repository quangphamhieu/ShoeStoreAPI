namespace ShoeStore.Domain.Entities
{
    public class PromotionProduct
    {
        public long Id { get; set; }
        public int PromotionId { get; set; }
        public int ProductId { get; set; }
        public decimal DiscountPercent { get; set; }

        public Promotion Promotion { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
