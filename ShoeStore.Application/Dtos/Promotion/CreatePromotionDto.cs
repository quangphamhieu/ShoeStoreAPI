namespace ShoeStore.Application.Dtos.Promotion
{
    public class CreatePromotionDto
    {
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusId { get; set; }

        public List<CreatePromotionProductDto>? Products { get; set; }
        public List<PromotionStoreDto>? Stores { get; set; }

    }

    public class CreatePromotionProductDto
    {
        public int ProductId { get; set; }
        public decimal DiscountPercent { get; set; }
    }
}
