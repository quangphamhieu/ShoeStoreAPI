namespace ShoeStore.Application.Dtos.Promotion
{
    public class PromotionProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? SKU { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
