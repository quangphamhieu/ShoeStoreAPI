namespace ShoeStore.Application.Dtos.Cart
{
    public class CartDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.Total);
    }
}