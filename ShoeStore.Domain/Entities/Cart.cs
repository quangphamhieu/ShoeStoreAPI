namespace ShoeStore.Domain.Entities;

public class Cart
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Status Status { get; set; } = null!;
    public ICollection<CartItem>? CartItems { get; set; }
}


