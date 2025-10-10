namespace ShoeStore.Domain.Entities;

public class Notification
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
