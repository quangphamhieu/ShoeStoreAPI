namespace ShoeStore.Domain.Entities;

public class User
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public Gender Gender { get; set; }
    public byte RoleId { get; set; }
    public int? StoreId { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Role Role { get; set; } = null!;
    public Store? Store { get; set; }
    public Status Status { get; set; } = null!;
}
public enum Gender
{
    Male,
    Female
}

