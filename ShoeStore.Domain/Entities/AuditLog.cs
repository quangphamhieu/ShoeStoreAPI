namespace ShoeStore.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string Action { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public long? RecordId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Description { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
