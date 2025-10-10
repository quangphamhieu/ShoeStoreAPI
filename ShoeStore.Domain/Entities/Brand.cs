namespace ShoeStore.Domain.Entities;

public class Brand
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int StatusId { get; set; }

    public Status Status { get; set; } = null!;
    public ICollection<Product>? Products { get; set; }
}
