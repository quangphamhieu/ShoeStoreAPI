namespace ShoeStore.Application.Dtos.Order
{
    public class OrderUpdateDto
    {
        public int StatusId { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}