namespace ShoeStore.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public long CustomerId { get; set; }
    public long? CreatedBy { get; set; }
    public int? StoreId { get; set; }
    public int StatusId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderType OrderType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User Customer { get; set; } = null!;
    public User? Creator { get; set; }
    public Store? Store { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<OrderDetail>? OrderDetails { get; set; }
}


public enum OrderType
{
    Online,
    Offline
}

public enum PaymentMethod
{
    Cash,
    Transfer
}
