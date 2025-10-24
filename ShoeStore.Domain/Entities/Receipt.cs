namespace ShoeStore.Domain.Entities;

public class Receipt
{
    public long Id { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public int SupplierId { get; set; }
    public long CreatedBy { get; set; }
    public int? StoreId { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public decimal TotalAmount { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public Store? Store { get; set; }
    public Status Status { get; set; } = null!;
    public ICollection<ReceiptDetail>? ReceiptDetails { get; set; }
}

