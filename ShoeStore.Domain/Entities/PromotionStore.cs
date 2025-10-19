namespace ShoeStore.Domain.Entities
{
    public class PromotionStore
    {
        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; } = null!;

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;
    }
}
