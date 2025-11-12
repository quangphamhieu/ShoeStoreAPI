namespace ShoeStore.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public long CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}