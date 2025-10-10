namespace ShoeStore.Domain.Entities;

public class Comment
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public int ProductId { get; set; }
    public string Content { get; set; } = null!;
    public byte? Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
