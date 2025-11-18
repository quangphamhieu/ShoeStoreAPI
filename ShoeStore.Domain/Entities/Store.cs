namespace ShoeStore.Domain.Entities;

public class Store
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Status Status { get; set; } = null!;

    public ICollection<User>? Users { get; set; }
    public ICollection<StoreProduct>? StoreProducts { get; set; }
    public ICollection<PromotionStore>? PromotionStores { get; set; }
    public ICollection<Order>? Orders { get; set; }
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}
