using ShoeStore.Domain.Entities;

public class CartItem
{
    public long Id { get; set; }
    public long CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}