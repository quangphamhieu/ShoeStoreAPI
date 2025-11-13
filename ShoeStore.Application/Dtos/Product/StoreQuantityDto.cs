namespace ShoeStore.Application.Dtos.Product
{
    public class StoreQuantityDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal? SalePrice { get; set; }
    }
}
