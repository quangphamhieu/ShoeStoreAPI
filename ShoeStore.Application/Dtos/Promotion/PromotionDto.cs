namespace ShoeStore.Application.Dtos.Promotion
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public List<PromotionProductDto>? Products { get; set; }
        public List<PromotionStoreDto>? Stores { get; set; }

    }
}
