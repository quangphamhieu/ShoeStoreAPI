namespace ShoeStore.Domain.Entities;

public class OrderDetail
{
    public long Id { get; set; }
    public long OrderId { get; set; }

    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }  // lấy từ StoreProduct.SalePrice tại thời điểm tạo

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
