namespace ShoeStore.Domain.Entities;

public class Role
{
    public byte Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<User>? Users { get; set; }
}
